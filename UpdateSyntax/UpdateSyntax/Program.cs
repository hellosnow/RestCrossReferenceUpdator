namespace UpdateSyntax
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: [repository root path, to scan for the markdown files]");
            }

            Regex rx = new Regex(@"\[(.*)\]\(.*\/docs-ref-autogen\/(.*\.yml)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var rootPath = args[0];

            var files = Directory.EnumerateFiles(rootPath, "**.md", SearchOption.AllDirectories);

            int count = 0;
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                string uid = string.Empty;

                var newContent = rx.Replace(content, (m) =>
                {
                    var ymlPath = Path.Combine(rootPath, "docs-ref-autogen", m.Groups[2].Value);
                    if (!File.Exists(ymlPath))
                    {
                        count++;
                        return m.Value;
                        //throw new FileNotFoundException($"Yaml path {ymlPath} not exists.");
                    }
                    else
                    {
                        using (FileStream inputStream = File.OpenRead(ymlPath))
                        {
                            using (StreamReader inputReader = new StreamReader(inputStream))
                            {
                                string text;
                                while (null != (text = inputReader.ReadLine()))
                                {
                                    if (text.StartsWith("uid: "))
                                    {
                                        uid = text.Substring(5).TrimEnd();
                                        if (string.IsNullOrEmpty(uid))
                                        {
                                            throw new InvalidOperationException($"UID {uid} should not be null or empty.");
                                        }
                                        break;
                                    }
                                }
                            }

                        }

                        var returnValue = $"[{m.Groups[1].Value}](xref:{uid})";
                        return returnValue;
                    }
                });

                if (!string.IsNullOrEmpty(uid))
                {
                    File.WriteAllText(file, newContent);
                    Console.WriteLine($"Updating {file}");
                }
            }
            Console.WriteLine($"total {count} yaml not found.");
        }
    }
}
