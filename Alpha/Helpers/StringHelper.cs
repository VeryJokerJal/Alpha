namespace Alpha.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// 将字符串的第一个字符转换为大写，其他字符转换为小写
        /// </summary>
        /// <param name="input">需要处理的字符串</param>
        /// <returns>返回处理后的字符串</returns>
        public static string CapitalizeFirstLetter(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;  // 如果输入为空或null，直接返回
            }

            // 将第一个字符转为大写，其余字符转为小写
            return char.ToUpper(input[0]) + input[1..].ToLower();
        }
    }
}
