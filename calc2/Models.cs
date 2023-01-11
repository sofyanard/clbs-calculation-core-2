using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace calc2
{
    public enum EnumPeriod
    {
        Bulanan = 1,
        Triwulanan = 3,
        Semesteran = 6,
        Tahunan = 12,
        JatuhTempo = 0
    }

    public enum EnumTransactionType
    {
        Pencairan = 0,
        PembayaranAngsuran = 1,
        PembayaranTunggakan = 2,
        PembayaranPokok = 3,
        PembayaranBunga = 4
    }

    public class Collect
    {
        public int Status { get; set; }
        public int MinDpd { get; set; }
        public int MaxDpd { get; set; }

        public Collect(int status, int minDpd, int maxDpd) 
        {
            this.Status = status;
            this.MinDpd = minDpd;
            this.MaxDpd = maxDpd;
        }
    }

    /*
    public static List<Collect> Collectability = new List<Collect>
    {
        new Collect(1, 0, 10),
        new Collect(2, 11, 90),
        new Collect(3, 91, 120),
        new Collect(4, 121, 180),
        new Collect(5, 181, 99999)
    };
    */

    public class Facility
    {
        public int Id { get; set; }
        public double Limit { get; set; }
        public int Tenor { get; set; }
        public double InterestRate { get; set; }
        public double DendaRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public bool? IsRevolving { get; set; }
        public EnumPeriod? PeriodePerhitunganPokok { get; set; }
        public EnumPeriod? PeriodePerhitunganBunga { get; set; }
        public double? BakiDebet { get; set; }
        public double? AvailableLimit { get; set; }
        public double? TotalPokok { get; set; }
        public double? TotalBunga { get; set; }
        public double? TotalDendaPokok { get; set; }
        public double? TotalDendaBunga { get; set; }
        public double? TotalLainnya { get; set; }
        public double? TotalKewajiban { get; set; }
        public int? Dpd { get; set; }
        public int? Kolektibilitas { get; set; }
        public int TanggalCutOff { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public EnumTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public double TransactionAmount { get; set; }

        public Transaction(int id, int facilityId, EnumTransactionType transactionType, DateTime transactionDate, double transactionAmount)
        {
            this.Id = id;
            this.FacilityId = facilityId;
            this.TransactionType = transactionType;
            this.TransactionDate = transactionDate;
            this.TransactionAmount = transactionAmount;
        }
    }

    public class Billing
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int? PeriodeKe { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DueDateX { get; set; }
        public double? Pokok { get; set; }
        public double? Bunga { get; set; }
        public double? DendaPokok { get; set; }
        public double? DendaBunga { get; set; }
        public double? Lainnya { get; set; }
        public int? Dpd { get; set; }
        public bool? IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public double? PaidAmount { get; set; }
        public int? TransactionId { get; set; }
    }

    public class PosisiBakiDebet
    {
        public long Id { get; set; }
        public int? PeriodeKe { get; set; }
        public DateTime MulaiBakiDebet { get; set; }
        public DateTime AkhirBakiDebet { get; set; }
        public int JumlahHariBakiDebet { get; set; }
        public double BakiDebet { get; set; }
        public double Bunga { get; set; }
        public double? JumlahBunga { get; set; }
    }
}
