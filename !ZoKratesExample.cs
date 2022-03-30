using System;
using System.Text;
using System.Numerics;
using System.Windows.Forms;
using System.Collections.Generic;
using JubJubCrypto;

namespace ZoKratesExample
{
	class Transaction
	{
		public readonly BigInteger Value;
		public readonly BigInteger Nonce;
		public readonly ECPoint Receiver;
		public readonly ECPoint Sender;
		public readonly ECPoint R;
		public readonly BigInteger S;

		public Transaction(Account sender, Account receiver, BigInteger value)
		{
			if (value.Sign <= 0)
				throw new Exception("Number of tokens is not positive.");
			if (value > sender.NewValue) 
				throw new Exception("Not enough tokens.");
			
			Sender = sender.Owner; Receiver = receiver.Owner;
			Value = value; Nonce = sender.NewNonce + 1;
			
			BigInteger[] message = new BigInteger[] { Value, Nonce, Receiver.X.Value, Receiver.Y.Value };
			EdDSA.Sign(sender.PrivateKey, message, out R, out S);
		}

		public override string ToString()
		{
			return $"{Value} {Nonce} {Receiver.X.Value} {Receiver.Y.Value} " +
				$"{Sender.X.Value} {Sender.Y.Value} {R.X.Value} {R.Y.Value} {S}";
		}
	}
	
	class Account
	{		
		public readonly ECPoint Owner;
		public readonly BigInteger PrivateKey;
		public BigInteger OldValue { get; protected set; }
		public BigInteger OldNonce { get; protected set; }
		public BigInteger NewValue { get; protected set; }
		public BigInteger NewNonce { get; protected set; }
		public Account(BigInteger privateKey, BigInteger value)
		{
			if (value.Sign < 0)
				throw new Exception("Number of tokens is negative.");

			Owner = EdDSA.GetPublicKey(privateKey);
			NewValue = OldValue = value;
			NewNonce = OldNonce = 0;
			PrivateKey = privateKey;
		}
		public void Commit()
		{
			OldValue = NewValue;
			OldNonce = NewNonce;
		}

		public Transaction Transfer(Account receiver, BigInteger value)
		{
			Transaction transaction = new Transaction(this, receiver, value);			
			NewValue -= value; receiver.NewValue += value; NewNonce++;
			return transaction;
		}

        public override string ToString()
        {
			return $"{Owner.X} {Owner.Y} {OldValue} {OldNonce} {NewValue} {NewNonce}";
        }
    }
	
	class Program
    {
		[STAThreadAttribute]
		static void Main(string[] args)
		{
			Account[] accounts = new Account[]
			{
				new Account(12, 1000),
				new Account(34, 100),
				new Account(56, 0),
				new Account(78, 10000),
				new Account(90, 7777)

			};

			List<Transaction> transactions = new List<Transaction>();
			transactions.Add(accounts[0].Transfer(accounts[1], 500));
			transactions.Add(accounts[1].Transfer(accounts[2], 400));
			transactions.Add(accounts[3].Transfer(accounts[2], 1000));
			transactions.Add(accounts[0].Transfer(accounts[2], 10));
			transactions.Add(accounts[4].Transfer(accounts[1], 73));
			transactions.Add(accounts[4].Transfer(accounts[1], 73));
			transactions.Add(accounts[2].Transfer(accounts[4], 20));
			transactions.Add(accounts[0].Transfer(accounts[3], 1));
			transactions.Add(accounts[4].Transfer(accounts[3], 1234));
			transactions.Add(accounts[2].Transfer(accounts[0], 7));

			StringBuilder text = new StringBuilder();

			for (int i = 0; i < transactions.Count; i++)
				text.Append($"{transactions[i]} ");

			for (int i = 0; i < accounts.Length; i++)
				text.Append($"{accounts[i]} ");

			for (int i = 2 * transactions.Count - accounts.Length; i > 0; i--)
				text.Append("0 0 0 0 0 0 ");

			Clipboard.SetText(text.ToString());
		}
	}
}