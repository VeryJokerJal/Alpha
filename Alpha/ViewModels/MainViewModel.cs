using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Alpha.Helpers;
using Alpha.Models;
using Alpha.Properties;
using Alpha.Views;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Alpha.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        [Reactive]
        public string? TestSummary { get; set; }
        [Reactive]
        public string? Code { get; set; }
        [Reactive]
        public string? BackTestButtonText { get; set; }
        [Reactive]
        public string? PauseBackTestButtonText { get; set; }
        [Reactive]
        public string? AlphaQuery { get; set; }
        [Reactive]
        public bool IsPaused { get; set; }
        [Reactive]
        public bool HasBackTestStarted { get; set; }
        [Reactive]
        public bool CanSubmit { get; set; }
        [Reactive]
        public WorldQuantAccountLoginResponse? WorldQuantAccountLoginResponse { get; set; }
        [Reactive]
        public ObservableCollection<Knowledge>? KnowledgeList { get; set; }
        [Reactive]
        public ObservableCollection<SimulationCompletedData>? SimulationCompletedDatas { get; set; }
        [Reactive]
        public Knowledge? SelectedKnowledge { get; set; }
        [Reactive]
        public bool IsKnowledgeLoading { get; set; }
        [Reactive]
        public CompetitionResponse? Competition { get; set; }
        [Reactive]
        public AlphaResponse? AlphaResponse { get; set; }
        public UserSettings UserSettings { get; }

        private IgnoreStatus _currentStatus;
        public IgnoreStatus CurrentStatus
        {
            get => _currentStatus;
            private set => this.RaiseAndSetIfChanged(ref _currentStatus, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand BackTestCommand { get; }
        public ICommand GetKnowledgeCommand { get; }
        public ICommand ViewAlphaDetailsCommand { get; }
        public ICommand PauseBackTestCommand { get; }
        public ICommand AlphaQueryCommand { get; }
        public ICommand SubmitCommand { get; }

        private HttpClientHandler? _httpClientHandler;
        private HttpClient? _httpClient;
        private List<SimulationData>? _alphaList;
        private CancellationTokenSource? _cts;
        private DispatcherTimer? dispatcherTimer;

        public MainViewModel()
        {
            UserSettings = Settings.User;
            base.PropertyChanged += MainViewModel_PropertyChanged;
            LoginCommand = ReactiveCommand.CreateFromTask(Login, this.WhenAnyValue(x => x.UserSettings.Email, x => x.UserSettings.Password, (email, password) => !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)), this);
            BackTestCommand = ReactiveCommand.Create(BackTest, this.WhenAny(x => x.WorldQuantAccountLoginResponse, x => x.Value != null), this);
            GetKnowledgeCommand = ReactiveCommand.CreateFromTask(LoadKnowledge, this.WhenAnyValue(x => x.WorldQuantAccountLoginResponse, y => y.IsKnowledgeLoading, (x, y) => x != null && !y), this);
            ViewAlphaDetailsCommand = ReactiveCommand.Create<AlphaResponse, Unit>(ViewAlphaDetails, outputScheduler: this);
            PauseBackTestCommand = ReactiveCommand.Create(PauseBackTest, this.WhenAny(x => x.WorldQuantAccountLoginResponse, x => x.Value != null), this);
            AlphaQueryCommand = ReactiveCommand.CreateFromTask(DoAlphaQuery, this.WhenAnyValue(x => x.AlphaQuery, x => x.WorldQuantAccountLoginResponse, (x, y) => !string.IsNullOrWhiteSpace(x) && y != null), this);
            SubmitCommand = ReactiveCommand.CreateFromTask(Submit, this.WhenAnyValue(x => x.WorldQuantAccountLoginResponse, x => x.AlphaResponse, (x, y) => x != null && y != null), this);
            PauseBackTestButtonText = "暂停";
            BackTestButtonText = "回测";
            Initialize();
        }

        private async Task Submit()
        {
            if (AlphaResponse?.Id == null)
            {
                _ = MessageBox.Show("Alpha 不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _ = await SubmitAlpha(AlphaResponse.Id)
                ? MessageBox.Show("提交成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information)
                : MessageBox.Show("提交失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PauseBackTest()
        {
            IsPaused = !IsPaused;
            PauseBackTestButtonText = IsPaused ? "继续" : "暂停";
        }

        private void Initialize()
        {
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
            dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMinutes(30)
            };
            dispatcherTimer.Tick += GetCompetitionTimer_Tick;
            TestSummary = "未回测";
            UpdateStatus();
            PreventSleep();
            if (!string.IsNullOrWhiteSpace(UserSettings.Email) && !string.IsNullOrWhiteSpace(UserSettings.Password))
            {
                _ = Login();
            }
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UserSettings.IgnoreWarnings):
                case nameof(UserSettings.IgnoreErrors):
                case nameof(UserSettings.IgnoreFailures):
                case nameof(UserSettings.IgnorePasses):
                case nameof(UserSettings.IgnoreCompleted):
                case nameof(UserSettings.IgnoreStop):
                    UpdateStatus();
                    break;
            }
        }

        private void GetCompetitionTimer_Tick(object? sender, EventArgs e)
        {
            _ = LoadCompetition();
        }

        public async Task DoAlphaQuery()
        {
            if (_httpClient == null || _httpClientHandler == null || WorldQuantAccountLoginResponse == null || string.IsNullOrWhiteSpace(AlphaQuery))
            {
                return;
            }

            HttpResponseMessage alphaResponse = await _httpClient.GetAsync($"https://api.worldquantbrain.com/alphas/{AlphaQuery}");
            string alphaResponseContent = await alphaResponse.Content.ReadAsStringAsync();
            AlphaResponse = JsonConvert.DeserializeObject<AlphaResponse>(alphaResponseContent);
            if (AlphaResponse != null)
            {
                CanSubmit = AlphaResponse?.Is?.Checks?.Any(delegate (AlphaCheck check)
                {
                    string? result = check.Result;
                    return result != null && (result.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                }) == false && AlphaResponse.Status?.ToUpper() == "UNSUBMITTED";
            }
            else
            {
                _ = MessageBox.Show("Alpha 不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Unit ViewAlphaDetails(AlphaResponse response)
        {
            AlphaInfoWindow alphaDetailsViewModel = new(response);
            alphaDetailsViewModel.Show();
            return default;
        }

        private async Task LoadCompetition()
        {
            if (_httpClient == null || _httpClientHandler == null || WorldQuantAccountLoginResponse == null)
            {
                return;
            }

            string json = await _httpClient.GetStringAsync("https://api.worldquantbrain.com/competitions/challenge");
            Competition = JsonConvert.DeserializeObject<CompetitionResponse>(json);
        }

        private void UpdateStatus()
        {
            CurrentStatus = IgnoreStatus.None;

            if (UserSettings.IgnoreWarnings)
            {
                CurrentStatus |= IgnoreStatus.Warning;
            }
            if (UserSettings.IgnoreErrors)
            {
                CurrentStatus |= IgnoreStatus.Error;
            }
            if (UserSettings.IgnoreFailures)
            {
                CurrentStatus |= IgnoreStatus.Failure;
            }
            if (UserSettings.IgnorePasses)
            {
                CurrentStatus |= IgnoreStatus.Pass;
            }
            if (UserSettings.IgnoreCompleted)
            {
                CurrentStatus |= IgnoreStatus.Complete;
            }
            if (UserSettings.IgnoreStop)
            {
                CurrentStatus |= IgnoreStatus.Stop;
            }
        }

        private async Task LoadKnowledge()
        {
            KnowledgeList = [];
            IsKnowledgeLoading = true;

            DatasetResponse? datasetResponse = await GetDataset();

            if (datasetResponse?.Results != null)
            {
                foreach (Dataset dataset in datasetResponse.Results)
                {
                    FieldResponse? fieldResponse = await GetField(dataset.Id);

                    if (fieldResponse?.Results != null)
                    {
                        Knowledge knowledge = new()
                        {
                            Title = dataset.Name,
                            Description = dataset.Description,
                            Fields = [],
                            FieldCount = dataset.FieldCount
                        };
                        KnowledgeList.Add(knowledge);
                        foreach (Field field in fieldResponse.Results)
                        {
                            knowledge.Fields.Add(field);
                        }
                    }
                }
            }
            IsKnowledgeLoading = false;
        }

        private async Task Relogin()
        {
            if (WorldQuantAccountLoginResponse?.Token?.Expiry.AddHours(-0.5) < DateTime.Now)
            {
                await Login();
            }
        }

        private async Task BackTest()
        {
            HasBackTestStarted = true;
            await Relogin();
            if (_httpClient == null || _httpClientHandler == null || WorldQuantAccountLoginResponse == null)
            {
                return;
            }
            BackTestButtonText = "停止";

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                await _cts.CancelAsync();
                _cts.Dispose();
                _cts = null;
                BackTestButtonText = "回测";
                TestSummary = "未回测";
                IsPaused = false;
                PauseBackTestButtonText = "暂停";
                return;
            }
            _cts = new();
            CancellationToken token = _cts.Token;

            int index = 0;
            TestSummary = "正在生成表达式...";
            await GenerateSimulations(token);
            if (_alphaList == null)
            {
                TestSummary = "请重试";
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            SimulationCompletedDatas = [];
            SemaphoreSlim semaphore = WorldQuantAccountLoginResponse.Permissions.Contains("CONSULTANT") ? new(10) : new(3);
            List<int> incoming = [];

            List<Task> tasks = _alphaList.Select(async item =>
            {
                await semaphore.WaitAsync();
                if (token.IsCancellationRequested)
                {
                    return;
                }
                int incomingIndex = index + 1;
                try
                {
                    await Relogin();

                    Stopwatch taskStopwatch = Stopwatch.StartNew();
                    index++;
                    int progress = index * 100 / _alphaList.Count;
                    incoming.Add(incomingIndex);
                    TestSummary = $"正在模拟第 {string.Join("、", incoming)} 个表达式，共 {_alphaList.Count} 个，进度{progress}% (用时 {stopwatch.Elapsed.FormatTime()})";
                    string json = JsonConvert.SerializeObject(item, Formatting.Indented);
                    StringContent content = new(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage simResponse = await _httpClient.PostAsync("https://api.worldquantbrain.com/simulations", content);
                    while (simResponse.StatusCode == HttpStatusCode.TooManyRequests && !token.IsCancellationRequested)
                    {
                        TestSummary = $"请求太频繁，20 秒后自动重试 (用时 {stopwatch.Elapsed.FormatTime()})";
                        while (IsPaused)
                        {
                            TestSummary = $"已暂停 (用时 {stopwatch.Elapsed.FormatTime()})";
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        await Task.Delay(TimeSpan.FromSeconds(20));
                        simResponse = await _httpClient.PostAsync("https://api.worldquantbrain.com/simulations", content);
                    }

                    if (simResponse.Headers.TryGetValues("Location", out IEnumerable<string>? simProgressUrls) && !token.IsCancellationRequested)
                    {
                        string? simProgressUrl = simProgressUrls?.FirstOrDefault();
                        int retryCount = 0;
                        const int maxRetries = 30;

                        while (retryCount < maxRetries && !token.IsCancellationRequested)
                        {
                            while (IsPaused)
                            {
                                TestSummary = $"已暂停 (用时 {stopwatch.Elapsed.FormatTime()})";
                                await Task.Delay(TimeSpan.FromSeconds(1));
                            }

                            HttpResponseMessage progressResponse = await _httpClient.GetAsync(simProgressUrl);

                            if (progressResponse.Headers.TryGetValues("Retry-After", out IEnumerable<string>? retryAfterValues))
                            {
                                string retryAfterStr = retryAfterValues.FirstOrDefault() ?? "0";
                                double retryAfterSec = double.Parse(retryAfterStr);

                                if (retryAfterSec == 0)
                                {
                                    string resultContent = await progressResponse.Content.ReadAsStringAsync();
                                    SimulationCompletedData? result = JsonConvert.DeserializeObject<SimulationCompletedData>(resultContent);
                                    if (result != null)
                                    {
                                        HttpResponseMessage alphaResponse = await _httpClient.GetAsync($"https://api.worldquantbrain.com/alphas/{result.Alpha}");
                                        string alphaResponseContent = await alphaResponse.Content.ReadAsStringAsync();
                                        AlphaResponse? alpha = JsonConvert.DeserializeObject<AlphaResponse>(alphaResponseContent);
                                        result.AlphaResult = alpha;
                                        if (UserSettings.AutoSubmit && alpha?.Is?.Checks?.Any(check => check.Result?.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase) == true) == false)
                                        {
                                            if (await SubmitAlpha(result.Alpha))
                                            {
                                                alpha.Status = "ACTIVE";
                                            }
                                        }
                                        result.Time = taskStopwatch.Elapsed.FormatTime();
                                        SimulationCompletedDatas.Add(result);
                                        break;
                                    }
                                }
                                await Task.Delay(TimeSpan.FromSeconds(retryAfterSec * 2));
                            }
                            else
                            {
                                string resultContent = await progressResponse.Content.ReadAsStringAsync();
                                SimulationCompletedData? result = JsonConvert.DeserializeObject<SimulationCompletedData>(resultContent);
                                if (result != null)
                                {
                                    HttpResponseMessage alphaResponse = await _httpClient.GetAsync($"https://api.worldquantbrain.com/alphas/{result.Alpha}");
                                    string alphaResponseContent = await alphaResponse.Content.ReadAsStringAsync();
                                    AlphaResponse? alpha = JsonConvert.DeserializeObject<AlphaResponse>(alphaResponseContent);
                                    result.Regular ??= item.Regular;
                                    result.AlphaResult = alpha?.Id == null ? null : alpha;
                                    if (UserSettings.AutoSubmit && alpha?.Is?.Checks?.Any(check => check.Result?.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase) == true) == false)
                                    {
                                        if (await SubmitAlpha(result.Alpha))
                                        {
                                            alpha.Status = "ACTIVE";
                                        }
                                    }
                                    result.Time = taskStopwatch.Elapsed.FormatTime();
                                    SimulationCompletedDatas.Add(result);
                                    break;
                                }
                            }
                            retryCount++;
                        }
                    }
                    taskStopwatch.Stop();
                }
                finally
                {
                    _ = incoming.Remove(incomingIndex);
                    _ = semaphore.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);
            await _cts.CancelAsync();
            stopwatch.Stop();
            TestSummary = $"完成 (用时 {stopwatch.Elapsed.FormatTime()})";
            BackTestButtonText = "回测";
            HasBackTestStarted = false;
        }

        private async Task GenerateSimulations(CancellationToken token = default)
        {
            _alphaList = [];

            if (KnowledgeList == null)
            {
                return;
            }

            if (UserSettings.BackTestModeIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    return;
                }

                //string alphaExpression = $"group_rank({datafield.Id}/cap, subindustry)";
                SimulationData simulationData = new()
                {
                    Type = "REGULAR",
                    Settings = new SimulationSettings
                    {
                        InstrumentType = UserSettings.SimulationSettings.AssetClass?.ToUpper(),
                        Region = UserSettings.SimulationSettings.Region?.ToUpper(),
                        Universe = UserSettings.SimulationSettings.Liquidity?.ToUpper(),
                        Delay = int.Parse(UserSettings.SimulationSettings.DecisionDelay ?? "1"),
                        Decay = int.Parse(UserSettings.SimulationSettings.LinearDecayWeightedAverage ?? "0"),
                        Neutralization = UserSettings.SimulationSettings.AlphaWeight?.ToUpper(),
                        Truncation = double.Parse(UserSettings.SimulationSettings.TruncationWeight ?? "0.08"),
                        Pasteurization = UserSettings.SimulationSettings.DataCleaning?.ToUpper(),
                        UnitHandling = UserSettings.SimulationSettings.WarningThrowing?.ToUpper(),
                        NanHandling = UserSettings.SimulationSettings.MissingValueHandling?.ToUpper(),
                        Language = "FASTEXPR",
                        Visualization = false
                    },
                    Regular = Code
                };

                _alphaList.Add(simulationData);
            }
            else if (UserSettings.BackTestModeIndex == 2)
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    return;
                }

                SearchScope searchScope = new(UserSettings.SimulationSettings.Region?.ToUpper(), UserSettings.SimulationSettings.DecisionDelay?.ToUpper(), UserSettings.SimulationSettings.Liquidity?.ToUpper(), UserSettings.SimulationSettings.Liquidity?.ToUpper());
                List<string> c1 = DirectiveReplacerExtension.HasPlaceholders(Code, 1) ? await GetDataFields(searchScope, search: "analyst", token: token) : [];
                List<string> c2 = DirectiveReplacerExtension.HasPlaceholders(Code, 2) ? await GetDataFields(searchScope, search: "fundamental", token: token) : [];
                List<string> c3 = DirectiveReplacerExtension.HasPlaceholders(Code, 3) ? await GetDataFields(searchScope, search: "model", token: token) : [];
                List<string> c4 = DirectiveReplacerExtension.HasPlaceholders(Code, 4) ? await GetDataFields(searchScope, search: "news", token: token) : [];
                List<string> c5 = DirectiveReplacerExtension.HasPlaceholders(Code, 5) ? await GetDataFields(searchScope, search: "option", token: token) : [];
                List<string> c6 = DirectiveReplacerExtension.HasPlaceholders(Code, 6) ? await GetDataFields(searchScope, search: "price volume", token: token) : [];
                List<string> c7 = DirectiveReplacerExtension.HasPlaceholders(Code, 7) ? await GetDataFields(searchScope, search: "social media", token: token) : [];

                Dictionary<int, List<string>> replacements = new()
                {
                    { 1, c1 },
                    { 2, c2 },
                    { 3, c3 },
                    { 4, c4 },
                    { 5, c5 },
                    { 6, c6 },
                    { 7, c7 }
                };
                foreach (string item in DirectiveReplacerExtension.ExpandPlaceholders(Code, replacements))
                {
                    SimulationData simulationData = new()
                    {
                        Type = "REGULAR",
                        Settings = new SimulationSettings
                        {
                            InstrumentType = UserSettings.SimulationSettings.AssetClass?.ToUpper(),
                            Region = UserSettings.SimulationSettings.Region?.ToUpper(),
                            Universe = UserSettings.SimulationSettings.Liquidity?.ToUpper(),
                            Delay = int.Parse(UserSettings.SimulationSettings.DecisionDelay ?? "1"),
                            Decay = int.Parse(UserSettings.SimulationSettings.LinearDecayWeightedAverage ?? "0"),
                            Neutralization = UserSettings.SimulationSettings.AlphaWeight?.ToUpper(),
                            Truncation = double.Parse(UserSettings.SimulationSettings.TruncationWeight ?? "0.08"),
                            Pasteurization = UserSettings.SimulationSettings.DataCleaning?.ToUpper(),
                            UnitHandling = UserSettings.SimulationSettings.WarningThrowing?.ToUpper(),
                            NanHandling = UserSettings.SimulationSettings.MissingValueHandling?.ToUpper(),
                            Language = "FASTEXPR",
                            Visualization = false
                        },
                        Regular = item
                    };
                    _alphaList.Add(simulationData);
                }
            }
            else
            {
                List<string> _groupCompareOp =
                [
                    "group_zscore",
                    //"group_backfill",
                    //"group_mean",
                    "group_rank",
                    "group_neutralize"
                ];
                List<string> _tsCompareOp =
                [
                    "ts_rank",          // 时间序列排名
                    "ts_zscore",        // 时间序列Z-score
                    "ts_av_diff",       // 时间序列平均差值
                    "ts_mean",          // 时间序列均值
                    "ts_std_dev",       // 时间序列标准差
                    "ts_sum",           // 时间序列总和
                    "ts_delta",         // 时间序列变化量
                    "ts_scale",         // 时间序列缩放
                    "ts_product",       
                    //"ts_corr",          // 时间序列相关性
                    "ts_decay_linear",  // 时间序列线性衰减
                    "ts_arg_min",
                    "ts_arg_max",
            ];
                List<string> _group =
                [
                    "market",
                    "industry",
                    "subindustry",
                    "sector",
                    "country",
                    "currency",
                    "exchange",
                ];
                List<string> _days =
                [
                    "300",
                    "30"
                ];
                List<string> alphaExpressions = [];
                List<string> companyFundamentals = await GetDataFields(new SearchScope(UserSettings.SimulationSettings.Region?.ToUpper(), UserSettings.SimulationSettings.DecisionDelay?.ToUpper(), UserSettings.SimulationSettings.Liquidity?.ToUpper(), UserSettings.SimulationSettings.Liquidity?.ToUpper()), datasetId: "fundamental6", token: token);

                await Parallel.ForEachAsync(_groupCompareOp, async (gco, cancellationToken) =>
                {
                    await Parallel.ForEachAsync(_tsCompareOp, async (tco, cancellationToken) =>
                    {
                        await Parallel.ForEachAsync(companyFundamentals, async (cf, cancellationToken) =>
                        {
                            await Parallel.ForEachAsync(_days, async (d, cancellationToken) =>
                            {
                                await Parallel.ForEachAsync(_group, async (grp, cancellationToken) =>
                                {
                                    string expression = $"{gco}({tco}({cf}, {d}), {grp})";
                                    alphaExpressions.Add(expression);
                                    await Task.CompletedTask;
                                });
                            });
                        });
                    });
                });

                foreach (string alphaExpression in alphaExpressions)
                {
                    //string alphaExpression = $"group_rank({datafield.Id}/cap, subindustry)";
                    SimulationData simulationData = new()
                    {
                        Type = "REGULAR",
                        Settings = new SimulationSettings
                        {
                            InstrumentType = UserSettings.SimulationSettings.AssetClass?.ToUpper(),
                            Region = UserSettings.SimulationSettings.Region?.ToUpper(),
                            Universe = UserSettings.SimulationSettings.Liquidity?.ToUpper(),
                            Delay = int.Parse(UserSettings.SimulationSettings.DecisionDelay ?? "1"),
                            Decay = int.Parse(UserSettings.SimulationSettings.LinearDecayWeightedAverage ?? "0"),
                            Neutralization = UserSettings.SimulationSettings.AlphaWeight?.ToUpper(),
                            Truncation = double.Parse(UserSettings.SimulationSettings.TruncationWeight ?? "0.08"),
                            Pasteurization = UserSettings.SimulationSettings.DataCleaning?.ToUpper(),
                            UnitHandling = UserSettings.SimulationSettings.WarningThrowing?.ToUpper(),
                            NanHandling = UserSettings.SimulationSettings.MissingValueHandling?.ToUpper(),
                            Language = "FASTEXPR",//FASTEXPR
                            Visualization = false
                        },
                        Regular = alphaExpression
                    };

                    _alphaList.Add(simulationData);
                }

                Random random = new();
                await Task.Run(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    _alphaList = [.. _alphaList.OrderBy(x => random.Next())];
                }, token);
            }
        }

        private async Task Login()
        {
            _httpClientHandler ??= new HttpClientHandler();
            _httpClientHandler.CookieContainer ??= new();
            _httpClient ??= new(_httpClientHandler);
            string url = "https://api.worldquantbrain.com/authentication";
            string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{UserSettings.Email}:{UserSettings.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync(url, null);
                _ = response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                WorldQuantAccountLoginResponse = JsonConvert.DeserializeObject<WorldQuantAccountLoginResponse>(responseBody);
                Debug.WriteLine("Login successful: " + responseBody);
                await LoadCompetition();
                await LoadKnowledge();
            }
            catch (HttpRequestException)
            {
                string errorContent = response != null ? await response.Content.ReadAsStringAsync() : "No response content";
                Debug.WriteLine("Login failed: " + errorContent);
            }
        }

        private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public async Task<DatasetResponse?> GetDataset()
        {
            if (_httpClient == null || _httpClientHandler == null)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync("https://api.worldquantbrain.com/data-sets?delay=1&instrumentType=EQUITY&limit=20&offset=0&region=USA&universe=TOP3000");
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DatasetResponse>(responseBody);
        }

        public async Task<FieldResponse?> GetField(string? id)
        {
            if (_httpClient == null || _httpClientHandler == null)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"https://api.worldquantbrain.com/data-fields?dataset.id={id}&delay=1&instrumentType=EQUITY&limit=50&offset=0&region=USA&universe=TOP3000");
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FieldResponse>(responseBody);
        }

        public async Task<List<string>> GetDataFields(SearchScope searchScope, string datasetId = "", string search = "", CancellationToken token = default)
        {
            if (_httpClient == null || _httpClientHandler == null)
            {
                return [];
            }

            string? instrumentType = searchScope.InstrumentType;
            string? region = searchScope.Region;
            string? delay = searchScope.Delay;
            string? universe = searchScope.Universe;

            string urlTemplate;

            urlTemplate = string.IsNullOrWhiteSpace(search)
                ? "https://api.worldquantbrain.com/data-fields?" +
                    $"&instrumentType={instrumentType}" +
                    $"&region={region}&delay={delay}&universe={universe}" +
                    $"&dataset.id={datasetId}&limit=50" +
                    "&offset={0}"
                : "https://api.worldquantbrain.com/data-fields?" +
                    $"&instrumentType={instrumentType}" +
                    $"&region={region}&delay={delay}&universe={universe}" +
                    $"&limit=50&search={search}" +
                    "&offset={0}";

            // Get count from initial request
            HttpResponseMessage countResponse = await _httpClient.GetAsync(string.Format(urlTemplate, 0));
            string countJson = await countResponse.Content.ReadAsStringAsync();
            int count = JsonDocument.Parse(countJson).RootElement.GetProperty("count").GetInt32();

            List<List<string>> datafieldsList = [];
            JsonElement results = JsonDocument.Parse(countJson).RootElement.GetProperty("results");

            List<string> batch = [];
            foreach (JsonElement result in results.EnumerateArray())
            {
                string? item = result.GetProperty("id").GetString();
                if (!string.IsNullOrWhiteSpace(item))
                {
                    batch.Add(item);
                }
            }
            datafieldsList.Add(batch);

            // Fetch data in batches
            for (int x = 50; x < count; x += 50)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                HttpResponseMessage response = await _httpClient.GetAsync(string.Format(urlTemplate, x));
                string content = await response.Content.ReadAsStringAsync();
                results = JsonDocument.Parse(content).RootElement.GetProperty("results");

                batch.Clear();
                foreach (JsonElement result in results.EnumerateArray())
                {
                    string? item = result.GetProperty("id").GetString();
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        batch.Add(item);
                    }
                }
                datafieldsList.Add(batch);
            }

            // Flatten the list
            List<string> datafieldsListFlat = datafieldsList.SelectMany(x => x).ToList();
            return datafieldsListFlat;
        }

        private async Task<bool> SubmitAlpha(string? alphaId)
        {
            if (_httpClient == null || _httpClientHandler == null || string.IsNullOrEmpty(alphaId))
            {
                return false;
            }

            string url = $"https://api.worldquantbrain.com/alphas/{alphaId}/submit";
            HttpResponseMessage response = await _httpClient.PostAsync(url, null);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            string? getUrl = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(getUrl))
            {
                return false;
            }

            while (true)
            {
                HttpResponseMessage getResponse = await _httpClient.GetAsync(getUrl);

                if (getResponse.Headers.TryGetValues("Retry-After", out IEnumerable<string>? retryAfterValues))
                {
                    string retryAfterStr = retryAfterValues.FirstOrDefault() ?? "0";
                    double retryAfterSec = double.Parse(retryAfterStr);
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterSec * 5));
                }
                else
                {
                    return getResponse.StatusCode != HttpStatusCode.Forbidden;
                }
            }
        }

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(ExecutionState esFlags);

        [Flags]
        private enum ExecutionState : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_AWAYMODE_REQUIRED = 0x00000040
        }

        private void PreventSleep()
        {
            _ = SetThreadExecutionState(ExecutionState.ES_CONTINUOUS | ExecutionState.ES_DISPLAY_REQUIRED);
        }
    }
}
