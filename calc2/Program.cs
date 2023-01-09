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
        facility.PeriodePerhitunganBunga = EnumPeriod.Triwulanan;
        facility.PeriodePerhitunganPokok = EnumPeriod.Triwulanan;
        facility.TanggalCutOff = 31;

        /************************/
        /*** Set-Up Transaksi ***/
        /************************/

        List<Transaction> listTransaction = new List<Transaction>()
        {
            new Transaction(1, 1, EnumTransactionType.Pencairan, new DateTime(2022, 1, 16), 3000000),
            new Transaction(2, 1, EnumTransactionType.Pencairan, new DateTime(2022, 3, 16), 2000000),
            new Transaction(3, 1, EnumTransactionType.Pencairan, new DateTime(2022, 5, 16), 1000000)
        };

        /******************/
        /*** Calulation ***/
        /******************/

        // Array of Due-Date
        DateTime[] arrDueDate = new DateTime[1];
        // Due-Date[0] => Tanggal Mulai
        arrDueDate[0] = facility.StartDate;

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
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }

        // Due-Date[N] => Maturity Date
        Array.Resize(ref arrDueDate, arrDueDate.Length + 1);
        arrDueDate[arrDueDate.Length - 1] = facility.MaturityDate;

        for (int i = 0; i < arrDueDate.Length - 1; i++)
        {
            logger.LogInformation("Calculation from: {0:d}, until: {1:d}", arrDueDate[i], arrDueDate[i+1]);

            // Cari semua transaksi dalam periode ini
            List<Transaction> listTransactionInPeriod = listTransaction
                .Where(x => x.TransactionDate >= arrDueDate[i] && x.TransactionDate <= arrDueDate[i+1])
                .ToList();

            foreach (Transaction transaction in listTransactionInPeriod)
            {
                logger.LogInformation("Transaction Id: {0}, Date: {1:d}, Type: {2}, Amount: {3:C}", transaction.Id, transaction.TransactionDate, transaction.TransactionType, transaction.TransactionAmount);
            }
        }

        logger.LogInformation("Program is completed!");
    }
}