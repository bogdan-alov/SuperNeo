using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace SuperNeo
{
	public class SuperNeo : SmartContract
	{

		//Token information
		public static readonly byte[] Owner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();
		public const string NAME = "SuperNeo";
		public const string SYMBOL = "SPN";
		public const int DECIMALS = 8;
		public static BigInteger TOTAL_SUPPLY = 100_000_000;

		public static string Symbol() => SYMBOL;
		public static string Name() => NAME;
		public static byte Decimals() => DECIMALS;
		public static BigInteger TotalSupply() => Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();

		public static event Action<byte[], byte[], BigInteger> Transferred;

		public static object Main(string method, params object[] args)
		{
			if (Runtime.Trigger == TriggerType.Verification)
			{
				if (Owner.Length == 20)
				{
					// if param Owner is script hash
					return Runtime.CheckWitness(Owner);
				}
			}
			else if (Runtime.Trigger == TriggerType.Application)
			{
				if (method == "totalSupply")
				{
					return TotalSupply();
				}
				if (method == "name")
				{
					return Name();
				}
				if (method == "symbol")
				{
					return Symbol();
				}
				if (method == "decimals")
				{
					return Decimals();
				}
				if (method == "balanceOf")
				{
					return BalanceOf((byte[])args[0]);
				}
				if (method == "init")
				{
					Init();
				}
				if (method == "transfer")
				{
					return Transfer((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
				}
			}
			return false;
		}

		

		public static bool Init()
		{
			var result = Storage.Get(Storage.CurrentContext, "initialised");

			if (result != null)
			{
				return false;
			}

			if (Runtime.CheckWitness(Owner))
			{
				Storage.Put(Storage.CurrentContext, Owner, TOTAL_SUPPLY);
				Storage.Put(Storage.CurrentContext, "totalSupply", TOTAL_SUPPLY);
				Storage.Put(Storage.CurrentContext, "initialised", "true");

				Transferred(null, Owner, TOTAL_SUPPLY);
				return true;
			}

			return false;
		}

		public static object Transfer(byte[] from, byte[] to, BigInteger amount)
		{
			if (amount <= 0) return false;
			if (!Runtime.CheckWitness(from)) return false;
			if (to.Length != 20) return false;

			BigInteger from_value = Storage.Get(Storage.CurrentContext, from).AsBigInteger();
			if (from_value < amount) return false;
			if (from == to) return true;
			if (from_value == amount)
				Storage.Delete(Storage.CurrentContext, from);
			else
				Storage.Put(Storage.CurrentContext, from, from_value - amount);
			BigInteger to_value = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
			Storage.Put(Storage.CurrentContext, to, to_value + amount);
			Transferred(from, to, amount);
			return true;
		}

		public static BigInteger BalanceOf(byte[] address)
		{
			Runtime.Notify("gettingBalance");
			if (address.Length != 20)
			{
				throw new Exception("Address is invalid.");
			}

			return Storage.Get(Storage.CurrentContext, address).AsBigInteger();
		}
	}
}