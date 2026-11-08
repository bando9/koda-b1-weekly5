using System.Globalization;

namespace Ewallet.Mvc.Models;

public class Wallet
{
    public string Id {get;private set;}
    public string WalletNumber {get;private set;}
    public string PINHash {get;private set;}
    public decimal Balance {get;private set;}
    public string UserId {get; private set;}
    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;
    

    public Wallet(string id, string walletNumber, string pinHash, string userId, decimal balance = 0)
    {        
        Id=id;
        WalletNumber=walletNumber;
        PINHash=pinHash;
        Balance = balance >= 0 ? balance : throw new ArgumentException("Saldo gaboleh kurang dari 0");
        UserId=userId;
    }

    public string GetInformation()
    {
        var formatToRuipah = CultureInfo.GetCultureInfo("id-ID");
        return $"Nomor Akun: {WalletNumber}\nPemilik: {UserId}\nSaldo: Rp.{Balance.ToString("C", formatToRuipah)} ";
    }

    // Mengurangi balance
    public decimal Debit(decimal amount)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("tambah saldo harus lebih dari 0");
        }
        if (amount > Balance)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Maaf, saldo dompet Anda tidak mencukupi");
        }
        
        return Balance-=amount;
    }
    
    // menambah balance
    public decimal Credit(decimal amount)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Jumlah penarikan saldo harus lebih dari 0");
        }
        UpdatedAt = DateTime.Now;
        return Balance+=amount;
    }

    public void UpdatePin(string newPinHash)
    {
        PINHash = newPinHash;
        UpdatedAt = DateTime.UtcNow;
    }

    
}