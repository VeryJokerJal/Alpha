using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DynamicData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alpha.Models
{
    public class AlphaMetrics
    {
        public string? DateCreated { get; set; }
        public double Sharpe { get; set; }
        public double Fitness { get; set; }
        public double Turnover { get; set; }
        public double Margin { get; set; }
        public double Decay { get; set; }
        public string? Code { get; set; }
    }

    public class WorldQuantClient
    {
        private static readonly List<string> BasicOps =
        [
            "log", "sqrt", "reverse", "inverse", "rank", "zscore", "log_diff", "s_log_1p",
            "fraction", "quantile", "normalize", "scale_down"
        ];

        private static readonly List<string> TsOps =
        [
            "ts_rank", "ts_zscore", "ts_delta", "ts_sum", "ts_product",
            "ts_ir", "ts_std_dev", "ts_mean", "ts_arg_min", "ts_arg_max", "ts_min_diff",
            "ts_max_diff", "ts_returns", "ts_scale", "ts_skewness", "ts_kurtosis",
            "ts_quantile"
        ];

        private static readonly List<string> TsNotUse =
        [
            "ts_min", "ts_max", "ts_delay", "ts_median"
        ];

        private static readonly List<string> Arsenal =
        [
            "ts_moment", "ts_entropy", "ts_min_max_cps", "ts_min_max_diff", "inst_tvr", "sigmoid",
            "ts_decay_exp_window", "ts_percentage", "vector_neut", "vector_proj", "signed_power"
        ];

        private static readonly List<string> TwinFieldOps =
        [
            "ts_corr", "ts_covariance", "ts_co_kurtosis", "ts_co_skewness", "ts_theilsen"
        ];

        private static readonly List<string> GroupOps =
        [
            "group_rank"
        ];

        private static readonly List<string> GroupAcOps =
        [
            "group_sum", "group_max", "group_mean", "group_median", "group_min", "group_std_dev"
        ];

        private static readonly List<string> OpsSet = BasicOps.Concat(TsOps).Concat(Arsenal).Concat(GroupOps).ToList();

        private readonly HttpClient _client;
        private readonly string _username;
        private readonly string _password;

        public WorldQuantClient(string username, string password)
        {
            _username = username;
            _password = password;
            _client = new HttpClient();
        }

        public async Task<HttpClient> LoginAsync()
        {
            byte[] byteArray = System.Text.Encoding.ASCII.GetBytes($"{_username}:{_password}");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            HttpResponseMessage response = await _client.PostAsync("https://api.worldquantbrain.com/authentication", null);
            string content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            return _client;
        }

        public async Task<AlphaMetrics?> LocateAlphaAsync(string alphaId)
        {
            while (true)
            {
                HttpResponseMessage response = await _client.GetAsync($"https://api.worldquantbrain.com/alphas/{alphaId}");
                if (response.Headers.Contains("Retry-After"))
                {
                    string? retryAfter = response.Headers.GetValues("Retry-After").FirstOrDefault();
                    if (double.TryParse(retryAfter, out double seconds))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(seconds));
                        continue;
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Logged out or error occurred.");
                    return null;
                }

                string stringContent = await response.Content.ReadAsStringAsync();
                JObject? metrics = JsonConvert.DeserializeObject<JObject>(stringContent);

                AlphaMetrics alphaMetrics = new()
                {
                    DateCreated = metrics?["dateCreated"]?.ToString(),
                    Sharpe = metrics?["is"]?["sharpe"]?.Value<double>() ?? 0,
                    Fitness = metrics?["is"]?["fitness"]?.Value<double>() ?? 0,
                    Turnover = metrics?["is"]?["turnover"]?.Value<double>() ?? 0,
                    Margin = metrics?["is"]?["margin"]?.Value<double>() ?? 0,
                    Decay = metrics?["settings"]?["decay"]?.Value<double>() ?? 0,
                    Code = metrics?["regular"]?["code"]?.ToString()
                };

                return alphaMetrics;
            }
        }

        public async Task SetAlphaPropertiesAsync(
            string alphaId,
            string? name = null,
            string? color = null,
            string selectionDesc = "None",
            string comboDesc = "None",
            List<string>? tags = null)
        {
            tags ??= ["ace_tag"];
            var paramsObj = new
            {
                color,
                name,
                tags,
                category = (object)null,
                regular = new { description = (object)null },
                combo = new { description = comboDesc },
                selection = new { description = selectionDesc }
            };

            StringContent jsonContent = new(JsonConvert.SerializeObject(paramsObj), System.Text.Encoding.UTF8, "application/json");
            _ = await _client.PatchAsync($"https://api.worldquantbrain.com/alphas/{alphaId}", jsonContent);
            // Handle response if necessary
        }

        public async Task<List<(string, double)>> CheckSubmissionAsync(List<string> alphaBag, List<(string, double)> goldBag, int start)
        {
            List<string> depot = [];
            _ = await LoginAsync();

            for (int idx = 0; idx < alphaBag.Count; idx++)
            {
                string g = alphaBag[idx];
                if (idx < start)
                {
                    continue;
                }

                if (idx % 5 == 0)
                {
                    Console.WriteLine(idx);
                }

                if (idx % 200 == 0)
                {
                    _ = await LoginAsync();
                }

                string? pc = await GetCheckSubmissionAsync(g);
                if (pc == "sleep")
                {
                    await Task.Delay(100000);
                    _ = await LoginAsync();
                    alphaBag.Add(g);
                }
                else if (double.IsNaN(double.Parse(pc)))
                {
                    Console.WriteLine("Check self-correlation error");
                    await Task.Delay(100000);
                    alphaBag.Add(g);
                }
                else if (pc == "fail")
                {
                    continue;
                }
                else if (pc == "error")
                {
                    depot.Add(g);
                }
                else
                {
                    Console.WriteLine(g);
                    goldBag.Add((g, double.Parse(pc)));
                }
            }

            Console.WriteLine(string.Join(", ", depot));
            return goldBag;
        }

        public async Task<string?> GetCheckSubmissionAsync(string alphaId)
        {
            HttpResponseMessage result;
            while (true)
            {
                result = await _client.GetAsync($"https://api.worldquantbrain.com/alphas/{alphaId}/check");
                if (result.Headers.Contains("Retry-After"))
                {
                    string? retryAfter = result.Headers.GetValues("Retry-After").FirstOrDefault();
                    if (double.TryParse(retryAfter, out double seconds))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(seconds));
                        continue;
                    }
                }
                break;
            }

            try
            {
                string json = await result.Content.ReadAsStringAsync();
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);

                if (obj["is"]?.Value<int>() == 0)
                {
                    Console.WriteLine("Logged out");
                    return "sleep";
                }

                if (obj["is"]?["checks"] is not JArray checks)
                {
                    return "error";
                }

                var checksDf = checks.Select(c => new
                {
                    Name = c["name"]?.ToString(),
                    Value = c["value"]?.ToString(),
                    Result = c["result"]?.ToString()
                }).ToList();

                string? pc = checksDf.FirstOrDefault(c => c.Name == "PROD_CORRELATION")?.Value;
                return !checksDf.Any(c => c.Result == "FAIL") ? pc : "fail";
            }
            catch
            {
                Console.WriteLine($"Catch: {alphaId}");
                return "error";
            }
        }

        public List<string> GetVecFields(List<string> fields)
        {
            List<string> vecOps =
            [
                "vec_avg", "vec_sum", "vec_ir", "vec_max", "vec_count", "vec_skewness", "vec_stddev", "vec_choose"
            ];
            List<string> vecFields = [];

            foreach (string field in fields)
            {
                foreach (string vecOp in vecOps)
                {
                    if (vecOp == "vec_choose")
                    {
                        vecFields.Add($"{vecOp}({field}, nth=-1)");
                        vecFields.Add($"{vecOp}({field}, nth=0)");
                    }
                    else
                    {
                        vecFields.Add($"{vecOp}({field})");
                    }
                }
            }

            return vecFields;
        }

        public async Task MultiSimulateAsync(List<List<(string, double)>> alphaPools, string neut, string region, string universe, int start)
        {
            _ = await LoginAsync();

            for (int x = start; x < alphaPools.Count; x++)
            {
                List<(string, double)> pool = alphaPools[x];
                List<string> progressUrls = [];

                for (int y = 0; y < pool.Count; y++)
                {
                    (string, double) task = pool[y];
                    List<object> simDataList = GenerateSimData(task, region, universe, neut);

                    try
                    {
                        HttpResponseMessage simulationResponse = await _client.PostAsJsonAsync("https://api.worldquantbrain.com/simulations", simDataList);
                        if (simulationResponse.Headers.Location != null)
                        {
                            progressUrls.Add(simulationResponse.Headers.Location.ToString());
                        }
                        else
                        {
                            Console.WriteLine($"Location header missing for simulation response: {simulationResponse.Content.ReadAsStringAsync().Result}");
                            await Task.Delay(600000); // Sleep for 10 minutes
                            _ = await LoginAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Location key error: {ex.Message}");
                        await Task.Delay(600000); // Sleep for 10 minutes
                        _ = await LoginAsync();
                    }
                }

                Console.WriteLine($"Pool {x} tasks posted.");

                foreach (string progress in progressUrls)
                {
                    try
                    {
                        HttpResponseMessage simulationProgress;
                        while (true)
                        {
                            simulationProgress = await _client.GetAsync(progress);
                            if (simulationProgress.Headers.Contains("Retry-After"))
                            {
                                string? retryAfter = simulationProgress.Headers.GetValues("Retry-After").FirstOrDefault();
                                if (double.TryParse(retryAfter, out double seconds))
                                {
                                    Console.WriteLine($"Sleeping for {seconds} seconds");
                                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        string progressContent = await simulationProgress.Content.ReadAsStringAsync();
                        JObject? progressJson = JsonConvert.DeserializeObject<JObject>(progressContent);
                        string? status = progressJson["status"]?.ToString();

                        if (status != "COMPLETE")
                        {
                            Console.WriteLine($"Not complete: {progress}");
                        }

                        // Handle children if necessary
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine($"Look into: {progress}");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Other exception occurred.");
                    }
                }

                Console.WriteLine($"Pool {x} simulations done.");
            }

            Console.WriteLine("Simulate done");
        }

        public List<object> GenerateSimData((string alpha, double decay) task, string region, string uni, string neut)
        {
            List<object> simDataList =
            [
                new
                {
                    type = "REGULAR",
                    settings = new
                    {
                        instrumentType = "EQUITY",
                        region,
                        universe = uni,
                        delay = 1,
                        task.decay,
                        neutralization = neut,
                        truncation = 0.08,
                        pasteurization = "ON",
                        unitHandling = "VERIFY",
                        nanHandling = "ON",
                        language = "FASTEXPR",
                        visualization = false
                    },
                    regular = task.alpha
                }
            ];

            return simDataList;
        }

        public List<List<(string, double)>> LoadTaskPool(List<(string alpha, double decay)> alphaList, int limitOfChildrenSimulations, int limitOfMultiSimulations)
        {
            List<List<(string, double)>> tasks = [];

            for (int i = 0; i < alphaList.Count; i += limitOfChildrenSimulations)
            {
                List<(string alpha, double decay)> task = alphaList.Skip(i).Take(limitOfChildrenSimulations).ToList();
                tasks.Add(task);
            }

            List<List<(string, double)>> pools = [];
            for (int i = 0; i < tasks.Count; i += limitOfMultiSimulations)
            {
                List<List<(string, double)>> pool = tasks.Skip(i).Take(limitOfMultiSimulations).ToList();
                pools.Add(pool);
            }

            return pools;
        }

        public async Task<List<dynamic>> GetDatasetsAsync(string instrumentType = "EQUITY", string region = "USA", int delay = 1, string universe = "TOP3000")
        {
            string url = $"https://api.worldquantbrain.com/data-sets?instrumentType={instrumentType}&region={region}&delay={delay}&universe={universe}";
            HttpResponseMessage response = await _client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            JArray? results = JsonConvert.DeserializeObject<JObject>(json)["results"] as JArray;
            return results.ToObject<List<dynamic>>();
        }

        public async Task<List<dynamic>> GetDataFieldsAsync(
            string instrumentType = "EQUITY",
            string region = "USA",
            int delay = 1,
            string universe = "TOP3000",
            string datasetId = "",
            string search = "")
        {
            string urlTemplate;
            int count;

            if (string.IsNullOrEmpty(search))
            {
                urlTemplate = $"https://api.worldquantbrain.com/data-fields?instrumentType={instrumentType}&region={region}&delay={delay}&universe={universe}&dataset.id={datasetId}&limit=50&offset={{0}}";
                HttpResponseMessage initialResponse = await _client.GetAsync(string.Format(urlTemplate, 0));
                string initialJson = await initialResponse.Content.ReadAsStringAsync();
                count = JsonConvert.DeserializeObject<JObject>(initialJson)["count"]?.Value<int>() ?? 0;
            }
            else
            {
                urlTemplate = $"https://api.worldquantbrain.com/data-fields?instrumentType={instrumentType}&region={region}&delay={delay}&universe={universe}&limit=50&search={search}&offset={{0}}";
                count = 100;
            }

            List<dynamic> dataFieldsList = [];

            for (int x = 0; x < count; x += 50)
            {
                string url = string.Format(urlTemplate, x);
                HttpResponseMessage response = await _client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();
                JArray? results = JsonConvert.DeserializeObject<JObject>(json)["results"] as JArray;
                dataFieldsList.AddRange(results.ToObject<List<dynamic>>());
            }

            return dataFieldsList;
        }

        public List<string> ProcessDataFields(List<dynamic> dataFields, string dataType)
        {
            List<string> dataFieldsFiltered;

            if (dataType == "matrix")
            {
                dataFieldsFiltered = dataFields
                    .Where(df => df.type == "MATRIX")
                    .Select(df => (string)df.id)
                    .ToList();
            }
            else if (dataType == "vector")
            {
                List<string> vectorFields = dataFields
                    .Where(df => df.type == "VECTOR")
                    .Select(df => (string)df.id)
                    .ToList();
                dataFieldsFiltered = GetVecFields(vectorFields);
            }
            else
            {
                dataFieldsFiltered = [];
            }

            List<string> tbFields = dataFieldsFiltered.Select(field => $"winsorize(ts_backfill({field}, 120), std=4)").ToList();
            return tbFields;
        }

        public async Task ViewAlphasAsync(List<(string, double)> goldBag)
        {
            _ = await LoginAsync();
            List<List<object>> sharpList = [];

            foreach ((string gold, double pc) in goldBag)
            {
                AlphaMetrics triple = await LocateAlphaAsync(gold);
                if (triple == null)
                {
                    continue;
                }

                List<object> info =
                [
                    triple.Sharpe,
                    triple.Fitness,
                    triple.Turnover,
                    triple.Margin,
                    triple.DateCreated,
                    triple.Decay,
                    pc
                ];

                sharpList.Add(info);
            }

            List<List<object>> sortedList = sharpList.OrderByDescending(x => (double)x[1]).ToList();
            foreach (List<object>? item in sortedList)
            {
                Console.WriteLine(string.Join(", ", item));
            }
        }

        public async Task<List<(string, string)>> GetAlphasAsync(
            string startDate,
            string endDate,
            double sharpeTh,
            double fitnessTh,
            string region,
            int alphaNum,
            string usage)
        {
            _ = await LoginAsync();
            List<(string, string)> output = [];
            int count = 0;

            for (int i = 0; i < alphaNum; i += 100)
            {
                Console.WriteLine(i);
                string urlE = $"https://api.worldquantbrain.com/users/self/alphas?limit=100&offset={i}" +
                           $"&status=UNSUBMITTED%1FIS_FAIL&dateCreated%3E=2024-{startDate}T00:00:00-04:00&dateCreated%3C=2024-{endDate}" +
                           $"T00:00:00-04:00&is.fitness%3E={fitnessTh}&is.sharpe%3E={sharpeTh}&settings.region={region}&order=-is.sharpe&hidden=false&type!=SUPER";

                string urlC = $"https://api.worldquantbrain.com/users/self/alphas?limit=100&offset={i}" +
                           $"&status=UNSUBMITTED%1FIS_FAIL&dateCreated%3E=2024-{startDate}T00:00:00-04:00&dateCreated%3C=2024-{endDate}" +
                           $"T00:00:00-04:00&is.fitness%3C-={fitnessTh}&is.sharpe%3C-={sharpeTh}&settings.region={region}&order=is.sharpe&hidden=false&type!=SUPER";

                List<string> urls = [urlE];
                if (usage != "submit")
                {
                    urls.Add(urlC);
                }

                foreach (string url in urls)
                {
                    HttpResponseMessage response = await _client.GetAsync(url);
                    try
                    {
                        JArray? alphaList = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())["results"] as JArray;
                        foreach (JToken alpha in alphaList)
                        {
                            string? alphaId = alpha["id"]?.ToString();
                            string? name = alpha["name"]?.ToString();
                            string? dateCreated = alpha["dateCreated"]?.ToString();
                            double sharpe = alpha["is"]?["sharpe"]?.Value<double>() ?? 0;
                            double fitness = alpha["is"]?["fitness"]?.Value<double>() ?? 0;
                            double turnover = alpha["is"]?["turnover"]?.Value<double>() ?? 0;
                            double margin = alpha["is"]?["margin"]?.Value<double>() ?? 0;
                            int longCount = alpha["is"]?["longCount"]?.Value<int>() ?? 0;
                            int shortCount = alpha["is"]?["shortCount"]?.Value<int>() ?? 0;
                            double decay = alpha["settings"]?["decay"]?.Value<double>() ?? 0;
                            string? exp = alpha["regular"]?["code"]?.ToString();

                            count++;

                            if ((longCount + shortCount) > 100)
                            {
                                if (sharpe < -sharpeTh)
                                {
                                    exp = "-" + exp;
                                }

                                List<object> rec =
                                [
                                    alphaId,
                                    exp,
                                    sharpe,
                                    turnover,
                                    fitness,
                                    margin,
                                    dateCreated,
                                    decay
                                ];

                                Console.WriteLine(string.Join(", ", rec));

                                // Additional logic based on turnover
                                // This part is not fully clear in Python code, adjust as needed
                                output.Add((alphaId, exp));
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"{i} finished re-login");
                        _ = await LoginAsync();
                    }
                }
            }

            Console.WriteLine($"count: {count}");
            return output;
        }

        public Dictionary<string, List<(string, double)>> Transform(List<(string, double)> nextAlphaRecs, string region)
        {
            List<(string, double)> output = [];

            foreach ((string, double) rec in nextAlphaRecs)
            {
                double decay = rec.Item2;
                string exp = rec.Item1;
                output.Add((exp, decay));
            }

            Dictionary<string, List<(string, double)>> outputDict = new()
            {
                { region, output }
            };

            return outputDict;
        }

        public List<(string, double)> Prune(List<(string, double)> nextAlphaRecs, string prefix, int keepNum)
        {
            List<(string, double)> output = [];
            Dictionary<string, int> numDict = [];

            foreach ((string, double) rec in nextAlphaRecs)
            {
                string exp = rec.Item1;
                string field = exp.Split(new[] { prefix }, StringSplitOptions.None).Last().Split(',')[0];
                double sharpe = rec.Item2;

                if (sharpe < 0)
                {
                    field = "-" + field;
                }

                if (!numDict.ContainsKey(field))
                {
                    numDict[field] = 0;
                }

                if (numDict[field] < keepNum)
                {
                    numDict[field]++;
                    output.Add(rec);
                }
            }

            return output;
        }

        public List<string> FirstOrderFactory(List<string> fields, List<string> opsSet)
        {
            List<string> alphaSet = [];

            foreach (string field in fields)
            {
                alphaSet.Add(field);

                foreach (string op in opsSet)
                {
                    if (op == "ts_percentage")
                    {
                        alphaSet.AddRange(TsCompFactory(op, field, "percentage", [0.5]));
                    }
                    else if (op == "ts_decay_exp_window")
                    {
                        alphaSet.AddRange(TsCompFactory(op, field, "factor", [0.5]));
                    }
                    else if (op == "ts_moment")
                    {
                        alphaSet.AddRange(TsCompFactory(op, field, "k", [2, 3, 4]));
                    }
                    else if (op == "ts_entropy")
                    {
                        alphaSet.AddRange(TsCompFactory(op, field, "buckets", [10]));
                    }
                    else if (op.StartsWith("ts_") || op == "inst_tvr")
                    {
                        alphaSet.AddRange(TsFactory(op, field));
                    }
                    else if (op.StartsWith("group_"))
                    {
                        alphaSet.AddRange(GroupFactory(op, field, "usa"));
                    }
                    else if (op.StartsWith("vector"))
                    {
                        alphaSet.AddRange(VectorFactory(op, field));
                    }
                    else if (op == "signed_power")
                    {
                        string alpha = $"{op}({field}, 2)";
                        alphaSet.Add(alpha);
                    }
                    else
                    {
                        string alpha = $"{op}({field})";
                        alphaSet.Add(alpha);
                    }
                }
            }

            return alphaSet;
        }

        public List<string> GetGroupSecondOrderFactory(List<string> firstOrder, List<string> groupOps, string region)
        {
            List<string> secondOrder = [];

            foreach (string fo in firstOrder)
            {
                foreach (string groupOp in groupOps)
                {
                    secondOrder.AddRange(GroupFactory(groupOp, fo, region));
                }
            }

            return secondOrder;
        }

        public List<string> GetTsSecondOrderFactory(List<string> firstOrder, List<string> tsOps)
        {
            List<string> secondOrder = [];

            foreach (string fo in firstOrder)
            {
                foreach (string tsOp in tsOps)
                {
                    secondOrder.AddRange(TsFactory(tsOp, fo));
                }
            }

            return secondOrder;
        }

        public List<string> GetDataFieldsCsv(string filename, string prefix)
        {
            List<string> collection = [];
            string[] lines = File.ReadAllLines(filename);

            foreach (string? line in lines.Skip(1)) // Skip header
            {
                string[] parts = line.Split(',');
                if (parts.Length > 0 && parts[0].StartsWith(prefix))
                {
                    collection.Add(parts[0]);
                }
            }

            return collection;
        }

        public List<string> TsArithFactory(string tsOp, string arithOp, string field)
        {
            string firstOrder = $"{arithOp}({field})";
            List<string> secondOrder = TsFactory(tsOp, firstOrder);
            return secondOrder;
        }

        public List<string> ArithTsFactory(string arithOp, string tsOp, string field)
        {
            List<string> secondOrder = [];
            List<string> firstOrder = TsFactory(tsOp, field);

            foreach (string fo in firstOrder)
            {
                secondOrder.Add($"{arithOp}({fo})");
            }

            return secondOrder;
        }

        public List<string> TsGroupFactory(string tsOp, string groupOp, string field, string region)
        {
            List<string> secondOrder = [];
            List<string> firstOrder = GroupFactory(groupOp, field, region);

            foreach (string fo in firstOrder)
            {
                secondOrder.AddRange(TsFactory(tsOp, fo));
            }

            return secondOrder;
        }

        public List<string> GroupTsFactory(string groupOp, string tsOp, string field, string region)
        {
            List<string> secondOrder = [];
            List<string> firstOrder = TsFactory(tsOp, field);

            foreach (string fo in firstOrder)
            {
                secondOrder.AddRange(GroupFactory(groupOp, fo, region));
            }

            return secondOrder;
        }

        public List<string> VectorFactory(string op, string field)
        {
            List<string> output = [];
            List<string> vectors = ["cap"];

            foreach (string vector in vectors)
            {
                string alpha = $"{op}({field}, {vector})";
                output.Add(alpha);
            }

            return output;
        }

        public List<string> TradeWhenFactory(string op, string field, string region)
        {
            List<string> output = [];

            List<string> openEvents =
            [
                "ts_arg_max(volume, 5) == 0", "ts_corr(close, volume, 20) < 0",
                "ts_corr(close, volume, 5) < 0", "ts_mean(volume,10)>ts_mean(volume,60)",
                "group_rank(ts_std_dev(returns,60), sector) > 0.7", "ts_zscore(returns,60) > 2",
                "ts_skewness(returns,120)> 0.7", "ts_arg_min(volume, 5) > 3",
                "ts_std_dev(returns, 5) > ts_std_dev(returns, 20)",
                "ts_arg_max(close, 5) == 0", "ts_arg_max(close, 20) == 0",
                "ts_corr(close, volume, 5) > 0", "ts_corr(close, volume, 5) > 0.3", "ts_corr(close, volume, 5) > 0.5",
                "ts_corr(close, volume, 20) > 0", "ts_corr(close, volume, 20) > 0.3", "ts_corr(close, volume, 20) > 0.5",
                $"ts_regression(returns, {field}, 5, lag = 0, rettype = 2) > 0",
                $"ts_regression(returns, {field}, 20, lag = 0, rettype = 2) > 0",
                "ts_regression(returns, ts_step(20), 20, lag = 0, rettype = 2) > 0",
                "ts_regression(returns, ts_step(5), 5, lag = 0, rettype = 2) > 0"
            ];

            List<string> exitEvents =
            [
                "abs(returns) > 0.1", "-1", "days_from_last_change(ern3_pre_reptime) > 20"
            ];

            // Define region-specific events if necessary
            // Currently, region is not used in the provided Python code's trade_when_factory

            foreach (string oe in openEvents)
            {
                foreach (string ee in exitEvents)
                {
                    string alpha = $"{op}({oe}, {field}, {ee})";
                    output.Add(alpha);
                }
            }

            return output;
        }

        public List<string> TsFactory(string op, string field)
        {
            List<string> output = [];
            List<int> days = [5, 22, 66, 120, 240];

            foreach (int day in days)
            {
                string alpha = $"{op}({field}, {day})";
                output.Add(alpha);
            }

            return output;
        }

        public List<string> TsCompFactory(string op, string field, string factor, List<double> paras)
        {
            List<string> output = [];
            List<int> l1 = [5, 22, 66, 240];
            var comb = from day in l1
                       from para in paras
                       select new { day, para };

            foreach (var pair in comb)
            {
                string alpha = $"{op}({field}, {pair.day}, {factor}={pair.para:F1})";
                output.Add(alpha);
            }

            return output;
        }

        public List<string> TwinFieldFactory(string op, string field, List<string> fields)
        {
            List<string> output = [];
            List<int> days = [5, 22, 66, 240];
            List<string> outset = fields.Except([field]).ToList();

            foreach (int day in days)
            {
                foreach (string? counterpart in outset)
                {
                    string alpha = $"{op}({field}, {counterpart}, {day})";
                    output.Add(alpha);
                }
            }

            return output;
        }

        public List<string> GroupFactory(string op, string field, string region)
        {
            List<string> output = [];
            List<string> vectors = ["cap"];

            // Define group lists based on region
            List<string> groups =
            [
                "market", "sector", "industry", "subindustry",
                "bucket(rank(fnd28_value_05480/close), range='0.2, 1, 0.2')",
                "bucket(rank(cap), range='0.1, 1, 0.1')",
                "bucket(group_rank(cap,sector),range='0,1,0.1')",
                "bucket(rank(ts_std_dev(ts_returns(close,1),20)),range = '0.1,1,0.1')"
            ];

            // Add region-specific groups
            switch (region.ToUpper())
            {
                case "CHN":
                    groups.AddRange([]);
                    break;
                case "TWN":
                    groups.AddRange([]);
                    break;
                case "ASI":
                    groups.AddRange([]);
                    break;
                case "USA":
                    groups.AddRange([]);
                    break;
                case "HKG":
                    groups.AddRange([]);
                    break;
                case "KOR":
                    groups.AddRange([]);
                    break;
                case "EUR":
                    groups.AddRange([]);
                    break;
                case "GLB":
                    groups.AddRange([]);
                    break;
                case "AMR":
                    groups.AddRange([]);
                    break;
                case "JPN":
                    groups.AddRange([]);
                    break;
                default:
                    break;
            }

            foreach (string group in groups)
            {
                if (op.StartsWith("group_vector"))
                {
                    foreach (string vector in vectors)
                    {
                        string alpha = $"{op}({field},{vector},densify({group}))";
                        output.Add(alpha);
                    }
                }
                else if (op.StartsWith("group_percentage"))
                {
                    string alpha = $"{op}({field},densify({group}),percentage=0.5)";
                    output.Add(alpha);
                }
                else
                {
                    string alpha = $"{op}({field},densify({group}))";
                    output.Add(alpha);
                }
            }

            return output;
        }
    }

    // Extension method for PATCH requests
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            HttpMethod method = new("PATCH");
            HttpRequestMessage request = new(method, requestUri)
            {
                Content = content
            };
            return await client.SendAsync(request);
        }
    }
}
