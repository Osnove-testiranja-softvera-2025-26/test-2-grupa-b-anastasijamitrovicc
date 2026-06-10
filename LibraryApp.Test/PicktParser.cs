using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using LibraryApp.Models;

namespace LibraryApp.Test
{
    public static class PictParser
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            // Traži Results.txt u korenskom folderu gde se pokreće test (bin/Debug/netX.X/)
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results.txt");

            // AKO FAJL NE POSTOJI: Vraća privremene test primere da kod ne bi "crveneo"
            if (!File.Exists(filePath))
            {
                yield return new TestCaseData(ActivityFrequency.High, 9, 25.0, false, 30);
                yield return new TestCaseData(ActivityFrequency.High, 9, 25.0, true, 25);
                yield return new TestCaseData(ActivityFrequency.High, 5, 15.0, false, 10);
                yield return new TestCaseData(ActivityFrequency.Regular, 11, 20.0, false, 20);
                yield return new TestCaseData(ActivityFrequency.Regular, 2, 28.0, false, 20);
                yield return new TestCaseData(ActivityFrequency.Regular, 2, 15.0, false, 10);
                yield break;
            }

            // AKO FAJL POSTOJI: Čita ga liniju po liniju (PICT odvaja kolone Tabom '\t')
            string[] lines = File.ReadAllLines(filePath);

            // Preskačemo prvi red (i = 1) jer je to zaglavlje (kolone) u PICT-u
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                // PICT podrazumevano odvaja kolone pomoću tabulatora (\t)
                string[] columns = lines[i].Split('\t');

                // Parsiranje podataka tačno onim redom kako su definisani u test metodi
                ActivityFrequency activityFrequency = (ActivityFrequency)Enum.Parse(typeof(ActivityFrequency), columns[0], true);
                int numOfPurchases = int.Parse(columns[1]);
                double bookPrice = double.Parse(columns[2]);
                bool penalty = bool.Parse(columns[3]);
                int expectedResult = int.Parse(columns[4]); // Očekivani popust (int)

                yield return new TestCaseData(activityFrequency, numOfPurchases, bookPrice, penalty, expectedResult);
            }
        }
    }
}