using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string fileName = "diary.txt";
        bool keepRunning = true;
        int attempts = 0;
        const int maxAttempts = 3;

        while (attempts < maxAttempts)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🛡️ SECURE DIARY LOGIN ===");
            Console.ResetColor();

            Console.Write($"Enter 4-digit PIN (Attempt {attempts + 1}/{maxAttempts}): ");
            string pin = "";

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pin += key.KeyChar;
                    Console.Write("*");
                }
            }

            if (pin == "1234")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Access Granted! Welcome back.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1000);
                break;
            }
            else
            {
                attempts++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Wrong PIN! ({maxAttempts - attempts} attempts left)");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);

                if (attempts == maxAttempts)
                {
                    Console.WriteLine("\nToo many failed attempts. App is locking for 10 seconds...");
                    for (int i = 10; i > 0; i--)
                    {
                        Console.Write($"\rLocked: {i}s remaining... ");
                        System.Threading.Thread.Sleep(1000);
                    }
                    attempts = 0;
                }
            }
        }

        while (keepRunning)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== 📓 MY DIGITAL DIARY ===");
            Console.ResetColor();
            Console.WriteLine("1. Write a new entry");
            Console.WriteLine("2. Read all entries");
            Console.WriteLine("3. Search entries by keyword");
            Console.WriteLine("4. Delete an entry");
            Console.WriteLine("5. View Statistics");
            Console.WriteLine("6. Exit");
            Console.Write("\nChoose an option: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    WriteEntry(fileName);
                    break;
                case "2":
                    ReadEntries(fileName);
                    break;
                case "3":
                    SearchEntries(fileName);
                    break;
                case "4":
                    DeleteEntry(fileName);
                    break;
                case "5":
                    ShowStatistics(fileName);
                    break;
                case "6":
                    keepRunning = false;
                    Console.WriteLine("Goodbye! Have a great day.");
                    System.Threading.Thread.Sleep(1000);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Press any key to try again...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    static void WriteEntry(string file)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- New Diary Entry ---");
        Console.ResetColor();

        Console.WriteLine("What's on your mind?");
        string content = Console.ReadLine() ?? "";

        int mood = 0;
        while (mood < 1 || mood > 5)
        {
            Console.Write("Rate your mood (1: Terrible, 5: Amazing): ");
            if (!int.TryParse(Console.ReadLine(), out mood) || mood < 1 || mood > 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a number between 1 and 5.");
                Console.ResetColor();
            }
        }
        string entry = $"[{DateTime.Now:dd/MM/yyyy HH:mm}] | Mood: {mood} | {content}";
        File.AppendAllLines(file, new List<string> { entry });
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nSaved! Your mood has been recorded. Press any key...");
        Console.ResetColor();
        Console.ReadKey();
    }

    static void ReadEntries(string file)
    {
        Console.WriteLine("\n--- YOUR DIARY HISTORY ---");
        if (File.Exists(file))
        {
            Console.WriteLine(File.ReadAllText(file));
        }
        else
        {
            Console.WriteLine("No entries found yet.");
        }
        Console.WriteLine("\nPress any key...");
        Console.ReadKey();
    }

    static void DeleteEntry(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine("No file found.");
            return;
        }

        List<string> lines = File.ReadAllLines(file).ToList();
        if (lines.Count == 0)
        {
            Console.WriteLine("Diary is empty.");
        }
        else
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {lines[i]}");
            }

            Console.Write("\nGive entry number to delete (0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= lines.Count)
            {
                Console.Write($"Are you sure? (y/n): ");
                if ((Console.ReadLine() ?? "").ToLower() == "y")
                {
                    lines.RemoveAt(index - 1);
                    File.WriteAllLines(file, lines);
                    Console.WriteLine("✅ Deleted.");
                }
            }
        }
        Console.ReadKey();
    }

    static void SearchEntries(string file)
    {
        Console.Write("\nEnter keyword: ");
        string keyword = (Console.ReadLine() ?? "").ToLower();
        if (File.Exists(file))
        {
            var results = File.ReadAllLines(file).Where(l => l.ToLower().Contains(keyword));
            if (results.Any())
            {
                foreach (var line in results) Console.WriteLine(line);
            }
            else
            {
                Console.WriteLine("No matches found.");
            }
        }
        Console.ReadKey();
    }

    static void ShowStatistics(string file)
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=== 📊 DIARY STATISTICS ===");
        Console.ResetColor();

        if (!File.Exists(file))
        {
            var lines = File.ReadAllLines(file);
            double totalMood = 0;
            int moodCount = 0;

            foreach (var line in lines)
            {
                if (line.Contains("Mood: "))
                {

                    int index = line.IndexOf("Mood: ") + 6;
                    if (int.TryParse(line.Substring(index, 1), out int val))
                    {
                        totalMood += val;
                        moodCount++;
                    }
                }
            }

            Console.WriteLine($"Total Entries: {lines.Length}");
            if (moodCount > 0)
            {
                double average = totalMood / moodCount;
                Console.Write("Average Mood: ");
        
                if (average >= 4) Console.ForegroundColor = ConsoleColor.Green;
                else if (average >= 2.5) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.Red;
        
                Console.WriteLine($"{average:F1} / 5.0");
                Console.ResetColor();
            }
        }
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }
}