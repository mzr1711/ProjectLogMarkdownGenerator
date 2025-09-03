using System.Text;

namespace MarkdownGenerator
{
    internal class Program
    {
        static string exePath = AppDomain.CurrentDomain.BaseDirectory;
        static string folderPath = Path.GetDirectoryName(exePath);
        static string folderName = Path.GetFileName(folderPath);

        static string headerDate = "**日期：**";
        static string headerStage = "**当前阶段：**";
        static string headerTotalCount = "### 目标完成总数：";
        static string headerYesterdayCount = "### 昨日完成目标数：";
        static string header_1 = "#### 一、今日开发目标";
        static string header_2 = "#### 二、昨日未完成开发目标";
        static string header_3 = "#### 三、近期开发目标";
        static string header_4 = "#### 四、项目优化";
        static string header_5 = "#### 五、感想";
        static string header_6 = "#### 六、已完成的任务";

        static void Main(string[] args)
        {
            try
            {
                DateTime today = DateTime.Now;
                string fileName = $"{folderName}_{today.Year}_{today.Month}_{today.Day}.md";
                string filePath = Path.Combine(exePath, fileName);

                DateTime yesterday = today.AddDays(-1);
                string yesterdayFileName = $"{folderName}_{yesterday.Year}_{yesterday.Month}_{yesterday.Day}.md";
                string yesterdayFilePath = Path.Combine(exePath, yesterdayFileName);

                if (File.Exists(filePath))
                {
                    Console.WriteLine("文件：" + fileName + "已存在");
                    Console.WriteLine("按任意键退出");
                    Console.ReadKey();
                    return;
                }

                string content = ReadYesterdayMarkdownContent(yesterdayFilePath, today);
                if (content == null)
                {
                    Console.WriteLine("昨日文件不存在，生成新文件");
                    content = GenerateNewMarkdownContent(today);
                }

                File.WriteAllText(filePath, content, Encoding.UTF8);
                Console.WriteLine("生成文件：" + fileName + "成功！");
                Console.WriteLine("按任意键退出");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("生成文件时出错" + ex.Message);
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        static string ReadYesterdayMarkdownContent(string filePath, DateTime today)
        {
            if (!File.Exists(filePath))
                return null;

            Console.WriteLine("读取昨日文件");

            List<string> lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();

            List<string> listUnresolvedTask = new List<string>();
            List<string> listResolvedTask = new List<string>();
            int taskIndex = -1;
            int yesterdayTaskIndex = -1;
            int finishedTaskIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                // 替换日期
                if (lines[i].Contains(headerDate))
                {
                    lines[i] = $"{headerDate}{today.Year}年{today.Month}月{today.Day}（{WeekToChinese(today.DayOfWeek)}）";
                    Console.WriteLine("替换日期完成");
                }

                // 查找下标位置
                if (lines[i].Contains(header_1))
                {
                    taskIndex = i;
                }

                // 查找下标位置
                if (lines[i].Contains(header_2))
                {
                    yesterdayTaskIndex = i;
                }

                // 查找下标位置
                if (lines[i].Contains(header_6))
                {
                    finishedTaskIndex = i;
                }
            }

            if (taskIndex != -1 && yesterdayTaskIndex != -1 && finishedTaskIndex != -1)
            {
                // 读取今日开发目标项添加进列表，并清空所有任务
                int index = 1;
                while (!lines[taskIndex + index].Contains("####"))
                {
                    if (lines[taskIndex + index].Contains("- [ ]"))
                    {
                        listUnresolvedTask.Add(lines[taskIndex + index]);
                        lines.RemoveAt(taskIndex + index);
                        yesterdayTaskIndex--;
                        finishedTaskIndex--;
                    }
                    else if (lines[taskIndex + index].Contains("- [x]"))
                    {
                        listResolvedTask.Add(lines[taskIndex + index]);
                        lines.RemoveAt(taskIndex + index);
                        yesterdayTaskIndex--;
                        finishedTaskIndex--;
                    }
                    else
                        index++;
                }

                // 删除已完成的昨日任务
                index = 1;
                while (!lines[yesterdayTaskIndex + index].Contains("####"))
                {
                    if (lines[yesterdayTaskIndex + index].Contains("- [x]"))
                    {
                        listResolvedTask.Add(lines[yesterdayTaskIndex + index]);
                        lines.RemoveAt(yesterdayTaskIndex + index);
                        finishedTaskIndex--;
                    }
                    else
                        index++;
                }

                // 将昨日未完成的当天任务添加进今日的昨日任务中
                index = 1;
                for (int i = listUnresolvedTask.Count - 1; i >= 0; i--)
                {
                    lines.Insert(yesterdayTaskIndex + index, listUnresolvedTask[i]);
                    finishedTaskIndex++;
                }

                // 将完成的任务添加到已完成的任务中
                index = 1;
                for (int i = listResolvedTask.Count - 1; i >= 0; i--)
                {
                    lines.Insert(finishedTaskIndex + index, listResolvedTask[i]);
                }
                DateTime yesterday = today.AddDays(-1);
                lines.Insert(finishedTaskIndex + index, $"- {yesterday.Year}年{yesterday.Month}月{yesterday.Day}（{WeekToChinese(yesterday.DayOfWeek)}）");
            }
            Console.WriteLine("任务更新完成");

            for (int i = 0; i < lines.Count; i++)
            {
                // 写入完成目标总数
                if (lines[i].Contains(headerTotalCount))
                {
                    int? count = GetNumberAfterText(lines[i], headerTotalCount);
                    count += listResolvedTask.Count;
                    lines[i] = headerTotalCount + count;
                }

                // 写入昨日完成目标数
                if (lines[i].Contains(headerYesterdayCount))
                {
                    lines[i] = headerYesterdayCount + listResolvedTask.Count;
                }
            }
            Console.WriteLine("写入目标数完成");

            // Environment.NewLine可以自适应操作系统换行规则，Windows使用\r\n，macOS和Linux使用\n
            return String.Join(Environment.NewLine, lines);
        }

        static string GenerateNewMarkdownContent(DateTime today)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"## {folderName}");
            sb.AppendLine($"{headerDate}{today.Year}年{today.Month}月{today.Day}日（{WeekToChinese(today.DayOfWeek)}）");
            sb.AppendLine(headerStage);
            sb.AppendLine(headerTotalCount + "0");
            sb.AppendLine(headerYesterdayCount + "0");
            sb.AppendLine(header_1);
            sb.AppendLine($"- [ ] ~~在此添加今日开发目标，勾选后第二天运行会自动添加进`已完成`部分，未勾选则会自动添加进`昨日任务`部分~~");
            sb.AppendLine(header_2);
            sb.AppendLine(header_3);
            sb.AppendLine("- [ ] ~~在此添加近期开发目标，此处不会进行自动更新，用于记录近期开发想法~~");
            sb.AppendLine(header_4);
            sb.AppendLine("- ~~在此添加项目优化、Bug修复等~~");
            sb.AppendLine(header_5);
            sb.AppendLine("~~在此添加感想~~");
            sb.AppendLine(header_6);

            return sb.ToString();
        }

        static string WeekToChinese(DayOfWeek week)
        {
            string chinese;
            switch (week)
            {
                case DayOfWeek.Monday: chinese = "星期一"; break;
                case DayOfWeek.Tuesday: chinese = "星期二"; break;
                case DayOfWeek.Wednesday: chinese = "星期三"; break;
                case DayOfWeek.Thursday: chinese = "星期四"; break;
                case DayOfWeek.Friday: chinese = "星期五"; break;
                case DayOfWeek.Saturday: chinese = "星期六"; break;
                case DayOfWeek.Sunday: chinese = "星期天"; break;
                default: chinese = "星期_"; break;
            }
            return chinese;
        }

        /// <summary>
        /// 用字符串操作提取特定文字后的数字
        /// </summary>
        static int? GetNumberAfterText(string input, string keyword)
        {
            // 检查输入是否有效
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(keyword))
            {
                return null;
            }

            // 1. 找到关键字在字符串中的位置
            int keywordIndex = input.IndexOf(keyword);
            if (keywordIndex == -1)
            {
                // 没找到关键字
                return null;
            }

            // 2. 计算关键字后面内容的起始位置
            int startIndex = keywordIndex + keyword.Length;
            if (startIndex >= input.Length)
            {
                // 关键字后面没有内容了
                return null;
            }

            // 3. 截取关键字后面的所有内容
            string afterKeyword = input.Substring(startIndex);

            // 4. 从截取的内容中提取连续的数字
            string numberStr = "";
            foreach (char c in afterKeyword)
            {
                if (char.IsDigit(c))
                {
                    // 如果是数字，就添加到结果中
                    numberStr += c;
                }
                else
                {
                    // 遇到非数字就停止（只取连续的数字）
                    break;
                }
            }

            // 5. 检查是否提取到了数字，并转换为int
            if (!string.IsNullOrEmpty(numberStr) && int.TryParse(numberStr, out int result))
            {
                return result;
            }

            return null;
        }
    }
}
