using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace CST_gokart
{
    class Versenyzo // Versenyző osztály
    {
        public string Vezeteknev { get; set; }
        public string Keresztnev { get; set; }
        public DateTime SzuletesiIdo { get; set; }
        public bool Elmult_e_18 { get; set; }
        public string Versenyzo_azonosito { get; set; }

        public Versenyzo(string vezeteknev, string keresztnev, DateTime szuletesiIdo, bool elmult_e_18, string versenyzo_azonosito)
        {
            Vezeteknev = vezeteknev;
            Keresztnev = keresztnev;
            SzuletesiIdo = szuletesiIdo;
            Elmult_e_18 = elmult_e_18;
            Versenyzo_azonosito = versenyzo_azonosito;
        }
    }

    class Foglalas // Foglalás osztály
    {
        public DateTime Kezdete { get; set; }
        public int Ora { get; set; }
        public List<Versenyzo> Versenyzok { get; set; }
    }

    internal class Program 
    {
        public static string RemoveDiacritics(string text) // Ékezetek eltávolítása
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized) //Karakterenként végigmegy a normalized nevű stringen, ebben az ékezetes karakterek szét lettek bontva alapbetűre + ékezetre.
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c); //Megnézi a karakter Unicode kategóriáját
                if (uc != UnicodeCategory.NonSpacingMark) //Ha a karakter nem ékezet, akkor hozzáadja a StringBuilder-hez
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC); //Visszaalakítja a StringBuilder tartalmát normál formára és visszaadja
        }

        static void Main(string[] args)
        {
            #region Köszöntő szöveg 
            string fejlec = "Gokart időpontfoglaló";
            for (int j = 0; j < fejlec.Length; j++)
            {
                Console.Write(fejlec[j]);
                Thread.Sleep(10);
            }
            Console.WriteLine();
            for (int i = 0; i < fejlec.Length; i++)
            {
                Console.Write("-");
                Thread.Sleep(10);
            }
            Console.WriteLine("\n");
            #endregion

            #region Elérhetőségek
            Console.WriteLine("Elérhetőségek:");
            Console.WriteLine("\t Név: GoKartTrack");
            Console.WriteLine("\t Cím: 1234 Budapest, Gokart utca 1.");
            Console.WriteLine("\t Telefon: +36 1 234 5678");
            Console.WriteLine("\t Weboldal: GoKartTrack.hu\n");
            #endregion

            #region Versenyzők adatai 
            string beolvas_v = File.ReadAllText("vezeteknevek.txt"); //Vezetéknevek beolvasása és eltárolása
            List<string> vezeteknevek = new List<string>();
            foreach (var item in beolvas_v.Split(','))
                vezeteknevek.Add(item.Trim('\'', ' '));

            string beolvas_k = File.ReadAllText("keresztnevek.txt"); //Keresztnevek beolvasása és eltárolása
            List<string> keresztnevek = new List<string>();
            foreach (var item in beolvas_k.Split(','))
                keresztnevek.Add(item.Trim('\'', ' '));

            Random rnd = new Random();

            Dictionary<string, Versenyzo> versenyzok = new Dictionary<string, Versenyzo>();
            for (int i = 1; i <= 150; i++) // 150 versenyző generálása
            {
                int randomIndex_v = rnd.Next(vezeteknevek.Count);
                int randomIndex_k = rnd.Next(keresztnevek.Count);

                string vezeteknev = vezeteknevek[randomIndex_v];
                string keresztnev = keresztnevek[randomIndex_k];
                DateTime szuletesiIdo = new DateTime(rnd.Next(1960, 2007), rnd.Next(1, 13), rnd.Next(1, 29));
                bool elmult_e_18 = (DateTime.Now.Year - szuletesiIdo.Year) >= 18;
                string versenyzo_azonosito = "GO" + i;

                Versenyzo uj_versenyzo = new Versenyzo(vezeteknev, keresztnev, szuletesiIdo, elmult_e_18, versenyzo_azonosito);
                versenyzok.Add($"uj_versenyzo{i}", uj_versenyzo);
            }
            #endregion

            #region Napi foglalás kiválasztása
            Console.Write("Melyik napra szeretnéd megnézni a foglalásokat? (ÉÉÉÉ-HH-NN formátumban): ");
            string datumInput = Console.ReadLine();
            DateTime foglalasNap;
            while (!DateTime.TryParseExact(datumInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out foglalasNap)) // formátum ellenőrzés
            {
                Console.WriteLine("Hibás formátum! Kérlek add meg újra (ÉÉÉÉ-HH-NN):");
                datumInput = Console.ReadLine();
            }

            DateTime nyitas = new DateTime(foglalasNap.Year, foglalasNap.Month, foglalasNap.Day, 8, 0, 0);
            DateTime zaras = new DateTime(foglalasNap.Year, foglalasNap.Month, foglalasNap.Day, 19, 0, 0);

            int foglalasokSzama = rnd.Next(3, 5); // 3–4 foglalás naponta
            List<Foglalas> foglalasok = new List<Foglalas>();

            for (int i = 0; i < foglalasokSzama; i++)
            {
                int foglaltOra = rnd.Next(1, 3); // 1 vagy 2 óra
                int fo = rnd.Next(8, 21); // 8-20 fő

                DateTime kezdete = nyitas.AddHours(rnd.Next(0, 11 - foglaltOra)); // véletlenszerű kezdés 8-19 óra között

                List<Versenyzo> foglaltVersenyzok = new List<Versenyzo>();
                List<int> indexek = new List<int>();
                while (indexek.Count < fo)
                {
                    int rndIndex = rnd.Next(1, 151);
                    if (!indexek.Contains(rndIndex))
                        indexek.Add(rndIndex);
                }
                foreach (int idx in indexek)
                    foglaltVersenyzok.Add(versenyzok[$"uj_versenyzo{idx}"]);

                foglalasok.Add(new Foglalas
                {
                    Kezdete = kezdete,
                    Ora = foglaltOra,
                    Versenyzok = foglaltVersenyzok
                });
            }

            Console.WriteLine($"\n\nFoglalások a napra {foglalasNap:yyyy-MM-dd}:\n");

            foreach (var f in foglalasok)
            {
                // Foglalás időpontja fehérrel
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Foglalás: {f.Kezdete:HH:mm} - {f.Kezdete.AddHours(f.Ora):HH:mm} | {f.Versenyzok.Count} versenyző");

                // Versenyzők adatai sárgával
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var v in f.Versenyzok)
                {
                    Console.WriteLine($"{v.Versenyzo_azonosito}-{RemoveDiacritics(v.Vezeteknev)}{RemoveDiacritics(v.Keresztnev)}-{v.SzuletesiIdo:yyyyMMdd} \n\t" +
                        $"{RemoveDiacritics(v.Vezeteknev.ToLower())}.{RemoveDiacritics(v.Keresztnev.ToLower())}@gmail.com");
                }
                Console.WriteLine();
            }

            // Szín visszaállítása
            Console.ResetColor();
            #endregion

            #region Kilépés
            Console.WriteLine("\nKilépéshez ENTER!");
            Console.ReadLine();
            #endregion
        }
    }
}