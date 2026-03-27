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
        string encryptedEntry = Encrypt(entry, 5); 
        File.AppendAllLines(file, new List<string> { encryptedEntry });
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nSaved! Your mood has been recorded. Press any key...");
        Console.ResetColor();
        Console.ReadKey();
    }

    static void ReadEntries(string file)
    {
        Console.Clear();
        Console.WriteLine("--- 📓 YOUR HISTORY (Decrypted) ---\n");

        if (File.Exists(file))
        {
            string[] encryptedLines = File.ReadAllLines(file);
            foreach (var line in encryptedLines)
            {
                string decryptedLine = Decrypt(line, 5);
                PrintWithMoodColor(decryptedLine);
            }
        }
        else 
        { 
            Console.WriteLine("No entries found."); 
        }
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }

    static void DeleteEntry(string file)
    {
        Console.Clear();
        Console.WriteLine("--- 🗑️ DELETE AN ENTRY ---");

        if (!File.Exists(file)) { Console.WriteLine("No entries found."); return; }

        List<string> encryptedLines = File.ReadAllLines(file).ToList();
    
        if (encryptedLines.Count == 0) { Console.WriteLine("Diary is empty."); return; }

        for (int i = 0; i < encryptedLines.Count; i++)
        {
            string decLine = Decrypt(encryptedLines[i], 5);
            Console.Write($"{i + 1}. ");
            PrintWithMoodColor(decLine); 
        }

        Console.Write("\nGive entry number to delete (0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= encryptedLines.Count)
        {
            Console.Write($"Are you sure you want to delete entry #{index}? (y/n): ");
            if ((Console.ReadLine() ?? "").ToLower() == "y")
            {
                encryptedLines.RemoveAt(index - 1); 
                File.WriteAllLines(file, encryptedLines); 
                Console.WriteLine("✅ Entry deleted successfully.");
            }
        }
        Console.ReadKey();
    }

    static void SearchEntries(string file)
        {
        Console.Clear();
        Console.WriteLine("--- 🔍 SEARCH ENTRIES ---");
        Console.Write("Enter keyword to find: ");
        string keyword = (Console.ReadLine() ?? "").ToLower();

        if (File.Exists(file))
        {
            string[] encryptedLines = File.ReadAllLines(file);
            bool found = false;

            foreach (var encLine in encryptedLines)
            {
                string decLine = Decrypt(encLine, 5);
            
                if (decLine.ToLower().Contains(keyword))
                {
                    PrintWithMoodColor(decLine);
                    found = true;
                }
            }

            if (!found) Console.WriteLine("❌ No matches found for your keyword.");
        }
        else { Console.WriteLine("Diary file not found."); }
    
        Console.WriteLine("\nPress any key...");
        Console.ReadKey();
    }

    static void ShowStatistics(string file)
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=== 📊 DIARY STATISTICS ===");
        Console.ResetColor();

        if (File.Exists(file))
        {
            var lines = File.ReadAllLines(file);
            double totalMood = 0;
            int moodCount = 0;
            int totalWords = 0;

            foreach (var encLine in lines)
            {   string line = Decrypt(encLine, 5);
                totalWords += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

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

            Console.WriteLine($"\nTotal Entries: {lines.Length}");
            Console.WriteLine($"Total Words Written: {totalWords}");

            if (moodCount > 0)
            {
                double average = totalMood / moodCount;
                Console.Write("Average Mood Score: ");
    
                if (average >= 4) Console.ForegroundColor = ConsoleColor.Green;
                else if (average >= 2.5) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.Red;
    
                Console.WriteLine($"{average:F1} / 5.0");
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("\nNo diary file found. Create your first entry!");
        }
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }
    static string Encrypt(string text, int key)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)(buffer[i] + key);
        }
        return new string(buffer);
    }

    static string Decrypt(string text, int key)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)(buffer[i] - key);
        }
        return new string(buffer);
    }
    static void PrintWithMoodColor(string line)
    {
        if (line.Contains("Mood: 5")) Console.ForegroundColor = ConsoleColor.Green;
        else if (line.Contains("Mood: 4")) Console.ForegroundColor = ConsoleColor.Cyan;
        else if (line.Contains("Mood: 1") || line.Contains("Mood: 2")) Console.ForegroundColor = ConsoleColor.Red;
        else Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine(line);
        Console.ResetColor();
    }
}