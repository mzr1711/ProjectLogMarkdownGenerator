using System.Text;

namespace MarkdownGenerator
{
    internal class Program
    {
        static string projectName = "像素塔防开发日志";
        static string savePath = "E:\\zhuomian\\";

        static void Main(string[] args)
        {
            try
            {
                DateTime today = DateTime.Now;
                string fileName = projectName + "_" + today.Year + "_" + today.Month + "_" + today.Day + ".md";
                string filePath = savePath + fileName;

                DateTime yesterday = today.AddDays(-1);
                string yesterdayFileName = $"{projectName}_{yesterday.Year}_{yesterday.Month}_{yesterday.Day}.md";
                string yesterdayFilePath = savePath + yesterdayFileName;

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

                //Console.WriteLine(content);
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

            List<string> lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();

            List<string> listUnresolvedTask = new List<string>();
            int taskIndex = -1;
            int yesterdayTaskIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                // 替换日期
                if (lines[i].Contains("**日期：**"))
                {
                    lines[i] = $"**日期：**{today.Year}年{today.Month}月{today.Day}（{WeekToChinese(today.DayOfWeek)}）";
                }

                // 清空今日完成目标数
                if (lines[i].Contains("### 今日完成目标数："))
                {
                    lines[i] = "### 今日完成目标数：0";
                }

                // 查找下标位置
                if (lines[i].Contains("#### 一、今日开发目标"))
                {
                    taskIndex = i;
                }

                // 查找下标位置
                if (lines[i].Contains("#### 二、昨日任务完成情况"))
                {
                    yesterdayTaskIndex = i;
                }
            }

            if (taskIndex != -1 && yesterdayTaskIndex != -1)
            {
                // 读取昨日未完成的任务添加进列表，并清空所有任务
                int i = 2;
                while (!lines[taskIndex + i].Contains("####"))
                {
                    if (lines[taskIndex + i].Contains("- [ ]"))
                    {
                        listUnresolvedTask.Add(lines[taskIndex + i]);
                        lines.RemoveAt(taskIndex + i);
                        yesterdayTaskIndex--;
                    }
                    else if (lines[taskIndex + i].Contains("- [x]"))
                    {
                        lines.RemoveAt(taskIndex + i);
                        yesterdayTaskIndex--;
                    }
                    else
                        i++;
                }

                // 删除已完成的昨日任务
                i = 2;
                while (!lines[yesterdayTaskIndex + i].Contains("####"))
                {
                    if (lines[yesterdayTaskIndex + i].Contains("- [x]"))
                        lines.RemoveAt(yesterdayTaskIndex + i);
                    else
                        i++;
                }

                // 将昨日未完成的当天任务添加进今日的昨日任务中
                i = 2;
                foreach (string task in listUnresolvedTask)
                {
                    lines.Insert(yesterdayTaskIndex + i, task);
                }
            }

            // Environment.NewLine可以自适应操作系统换行规则，Windows使用\r\n，macOS和Linux使用\n
            return String.Join(Environment.NewLine, lines);
        }

        static string GenerateNewMarkdownContent(DateTime today)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("## 像素塔防开发日志");
            sb.AppendLine($"**日期：**{today.Year}年{today.Month}月{today.Day}日（{WeekToChinese(today.DayOfWeek)}）");
            sb.AppendLine("**当前阶段：**");
            sb.AppendLine("### 当前目标完成总数：0");
            sb.AppendLine("### 今日完成目标数：0");
            sb.AppendLine("#### 一、今日开发目标");
            sb.AppendLine("- [ ] ~~等待添加~~");
            sb.AppendLine("#### 二、昨日任务完成情况");
            sb.AppendLine("- [ ] ~~等待添加~~");
            sb.AppendLine("#### 三、近期开发目标");
            sb.AppendLine("- [ ] ~~等待添加~~");
            sb.AppendLine("#### 四、项目优化");
            sb.AppendLine("-  ~~等待添加~~");
            sb.AppendLine("#### 五、感想");

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
    }
}
