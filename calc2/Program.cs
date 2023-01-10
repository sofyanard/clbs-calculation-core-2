using calc2;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

public class Program
{
    private static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Program is starting...");

        /************************/
        /*** Set-Up Fasilitas ***/
        /************************/

        Facility facility = new Facility();
        facility.Id = 1;
        facility.Limit = 10000000;
        facility.Tenor = 12;
        facility.InterestRate = 12;
        facility.StartDate = new DateTime(2022, 1, 1);
        facility.MaturityDate = new DateTime(2023, 1, 1);
        facility.IsRevolving = true;
        facility.PeriodePerhitunganBunga = EnumPeriod.Bulanan;
        facility.PeriodePerhitunganPokok = EnumPeriod.Triwulanan;
        facility.TanggalCutOff = 31;

        facility.BakiDebet = 0;
        facility.AvailableLimit = facility.Limit;

        /************************/
        /*** Set-Up Transaksi ***/
        /************************/

        List<Transaction> listTransaction = new List<Transaction>()
        {
            new Transaction(1, 1, EnumTransactionType.Pencairan, new DateTime(2022, 1, 16), 3000000),
            new Transaction(2, 1, EnumTransactionType.Pencairan, new DateTime(2022, 3, 16), 2000000),
            new Transaction(3, 1, EnumTransactionType.Pencairan, new DateTime(2022, 5, 16), 1000000),
            new Transaction(3, 1, EnumTransactionType.Pencairan, new DateTime(2022, 5, 26), 2000000)
        };

        /******************/
        /*** Calulation ***/
        /******************/

        // Array of Due-Date
        DateTime[] arrDueDate = new DateTime[1];
        // Due-Date[0] => Tanggal Mulai
        arrDueDate[0] = facility.StartDate;
        logger.LogInformation("arrDueDate[0]: {0:d}", arrDueDate[0]);

        if ((facility.TanggalCutOff < 1) || (facility.TanggalCutOff > 31))
        {
            throw new Exception("Tanggal Cut-Off tidak valid!");
        }

        // Due-Date[1] s/d Due-Date[N-1]
        DateTime nextDueDate;
        int n = 0, nextYear, nextMonth;
        int s = (int)facility.PeriodePerhitunganBunga;
        try
        {
            nextYear = arrDueDate[0].Year;
            nextMonth = arrDueDate[0].Month;

            nextDueDate = new DateTime(nextYear, nextMonth,
                    (facility.TanggalCutOff > DateTime.DaysInMonth(nextYear, nextMonth)) ? DateTime.DaysInMonth(nextYear, nextMonth) : facility.TanggalCutOff);

            if (nextDueDate <= arrDueDate[0])
            {
                nextMonth = nextMonth + s;
                if (nextMonth > 12)
                {
                    nextYear++;
                    nextMonth = nextMonth % 12;
                }

                nextDueDate = new DateTime(nextYear, nextMonth,
                    (facility.TanggalCutOff > DateTime.DaysInMonth(nextYear, nextMonth)) ? DateTime.DaysInMonth(nextYear, nextMonth) : facility.TanggalCutOff);
            }

            while (nextDueDate < facility.MaturityDate)
            {
                if (n > 0)
                {
                    nextYear = nextDueDate.Year;
                    nextMonth = nextDueDate.Month + s;
                    if (nextMonth > 12)
                    {
                        nextYear++;
                        nextMonth = nextMonth % 12;
                    }

                    logger.LogInformation("n={0}, y={1}, m={2}", n, nextYear, nextMonth);

                    nextDueDate = new DateTime(nextYear, nextMonth,
                    (facility.TanggalCutOff > DateTime.DaysInMonth(nextYear, nextMonth)) ? DateTime.DaysInMonth(nextYear, nextMonth) : facility.TanggalCutOff);
                }

                if (nextDueDate < facility.MaturityDate)
                {
                    n++;
                    Array.Resize(ref arrDueDate, arrDueDate.Length + 1);
                    arrDueDate[n] = nextDueDate;
                    logger.LogInformation("arrDueDate[{n}]: {1:d}", n, arrDueDate[n]);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError("Creating array of due date error: {0}", e.Message);
        }

        // Due-Date[N] => Maturity Date
        Array.Resize(ref arrDueDate, arrDueDate.Length + 1);
        arrDueDate[arrDueDate.Length - 1] = facility.MaturityDate;
        logger.LogInformation("arrDueDate[{n}]: {1:d}", arrDueDate.Length - 1, arrDueDate[arrDueDate.Length - 1]);

        logger.LogInformation("Finish creating array of due date");
        
        n = 0;
        PosisiBakiDebet[] arrPosisiBakiDebet = new PosisiBakiDebet[1];
        arrPosisiBakiDebet[0] = new PosisiBakiDebet();
        logger.LogInformation("Add arrPosisiBakiDebet[0]");
        arrPosisiBakiDebet[0].Id = n;
        arrPosisiBakiDebet[0].MulaiBakiDebet = arrDueDate[0];
        arrPosisiBakiDebet[0].BakiDebet = 0;

        for (int i = 0; i < arrDueDate.Length - 1; i++)
        {
            logger.LogInformation("Starting period: {0:d}", arrDueDate[i]);

            // Awal Periode
            if (n > 0)
            {
                Array.Resize(ref arrPosisiBakiDebet, arrPosisiBakiDebet.Length + 1);
                n++;
                arrPosisiBakiDebet[n] = new PosisiBakiDebet();
                logger.LogInformation("Add arrPosisiBakiDebet[{0}]", n);
                arrPosisiBakiDebet[n].Id = n;
                arrPosisiBakiDebet[n].MulaiBakiDebet = arrDueDate[i];
                arrPosisiBakiDebet[n].BakiDebet = arrPosisiBakiDebet[n - 1].BakiDebet;
            }
            
            // Cari semua transaksi dalam periode ini
            List<Transaction> listTransactionInPeriod;
            if (i == 0)
            {
                listTransactionInPeriod = listTransaction
                .Where(x => x.TransactionDate >= arrDueDate[i] && x.TransactionDate <= arrDueDate[i + 1])
                .OrderBy(x => x.TransactionDate).ToList();
            }
            else
            {
                listTransactionInPeriod = listTransaction
                .Where(x => x.TransactionDate > arrDueDate[i] && x.TransactionDate <= arrDueDate[i + 1])
                .OrderBy(x => x.TransactionDate).ToList();
            }

            logger.LogInformation("Listing transaction from: {0:d} to: {1:d}, found: {2}", arrDueDate[i], arrDueDate[i + 1], listTransactionInPeriod.Count);

            double bunga;
            double interest = facility.InterestRate / 100;

            foreach (Transaction transaction in listTransactionInPeriod)
            {
                logger.LogInformation("Transaction Id: {0}, Date: {1:d}, Type: {2}, Amount: {3:C}", transaction.Id, transaction.TransactionDate, transaction.TransactionType, transaction.TransactionAmount);

                switch (transaction.TransactionType)
                {
                    case EnumTransactionType.Pencairan:
                        Array.Resize(ref arrPosisiBakiDebet, arrPosisiBakiDebet.Length + 1);
                        n++;
                        arrPosisiBakiDebet[n] = new PosisiBakiDebet();
                        logger.LogInformation("Add arrPosisiBakiDebet[{0}]", n);
                        arrPosisiBakiDebet[n].Id = n;

                        // Hitung arrPosisiBakiDebet[N-1]
                        arrPosisiBakiDebet[n - 1].AkhirBakiDebet = transaction.TransactionDate;
                        logger.LogInformation("arrPosisiBakiDebet[{0}].AkhirBakiDebet = {1:d}", n - 1, arrPosisiBakiDebet[n - 1].AkhirBakiDebet);

                        arrPosisiBakiDebet[n - 1].JumlahHariBakiDebet = (arrPosisiBakiDebet[n - 1].AkhirBakiDebet - arrPosisiBakiDebet[n - 1].MulaiBakiDebet).Days;
                        logger.LogInformation("arrPosisiBakiDebet[{0}].JumlahHariBakiDebet = {1:d}", n - 1, arrPosisiBakiDebet[n - 1].JumlahHariBakiDebet);

                        bunga = ((double)arrPosisiBakiDebet[n - 1].JumlahHariBakiDebet / (double)360.0) * interest * (double)arrPosisiBakiDebet[n - 1].BakiDebet;
                        arrPosisiBakiDebet[n - 1].Bunga = bunga;
                        logger.LogInformation("arrPosisiBakiDebet[{0}].Bunga = {1:C}", n - 1, arrPosisiBakiDebet[n - 1].Bunga);

                        // Hitung Fasilitas
                        facility.BakiDebet += transaction.TransactionAmount;
                        logger.LogInformation("facility.BakiDebet = {0:C}", facility.BakiDebet);

                        facility.AvailableLimit -= transaction.TransactionAmount;
                        logger.LogInformation("facility.AvailableLimit = {0:C}", facility.AvailableLimit);

                        // Hitung arrPosisiBakiDebet[N]
                        arrPosisiBakiDebet[n].MulaiBakiDebet = transaction.TransactionDate;
                        logger.LogInformation("arrPosisiBakiDebet[{0}].MulaiBakiDebet = {1:d}", n, arrPosisiBakiDebet[n].MulaiBakiDebet);

                        arrPosisiBakiDebet[n].BakiDebet = (double)facility.BakiDebet;
                        logger.LogInformation("arrPosisiBakiDebet[{0}].BakiDebet = {1:C}", n, arrPosisiBakiDebet[n].BakiDebet);

                        break;
                    case EnumTransactionType.PembayaranPokok:

                        break;
                    case EnumTransactionType.PembayaranBunga:

                        break;
                    default:
                        break;
                }
            }

            // Akhir Periode
            arrPosisiBakiDebet[n].AkhirBakiDebet = arrDueDate[i + 1];
            logger.LogInformation("arrPosisiBakiDebet[{0}].AkhirBakiDebet = {1:d}", n, arrPosisiBakiDebet[n].AkhirBakiDebet);

            arrPosisiBakiDebet[n].JumlahHariBakiDebet = (arrPosisiBakiDebet[n].AkhirBakiDebet - arrPosisiBakiDebet[n].MulaiBakiDebet).Days;
            logger.LogInformation("arrPosisiBakiDebet[{0}].JumlahHariBakiDebet = {1:d}", n, arrPosisiBakiDebet[n].JumlahHariBakiDebet);

            bunga = ((double)arrPosisiBakiDebet[n].JumlahHariBakiDebet / (double)360.0) * interest * (double)arrPosisiBakiDebet[n].BakiDebet;
            arrPosisiBakiDebet[n].Bunga = bunga;
            logger.LogInformation("arrPosisiBakiDebet[{0}].Bunga = {1:C}", n, arrPosisiBakiDebet[n].Bunga);
        }

        logger.LogInformation("Finish creating array of posisi bakidebet!");

        for (int i = 0; i < arrPosisiBakiDebet.Length; i++)
        {
            logger.LogInformation("{0} | {1:d} | {2:d} | {3} | {4:C} | {5:C}", arrPosisiBakiDebet[i].Id, arrPosisiBakiDebet[i].MulaiBakiDebet, arrPosisiBakiDebet[i].AkhirBakiDebet, arrPosisiBakiDebet[i].JumlahHariBakiDebet, arrPosisiBakiDebet[i].BakiDebet, arrPosisiBakiDebet[i].Bunga);
        }

        logger.LogInformation("Program is completed!");
    }
}