using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Tx;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class AccountSequenceCache
	{
		public AccountSequenceCache (string account)
		{
			this.Account = account;
			this.SequenceCache = Load (account);
		}

		private readonly string Account;
		public void UpdateOrdersCache (AutomatedOrder order /*, string account*/)
		{

			lock (lockobj) {
				if (order == null) {
					return;
				}

				string id = order.Bot_ID;
				Logging.WriteLog ("Synccache : " + (id ?? "null"));

				Dictionary<string, AutomatedOrder> dict = this.SequenceCache;
				if (dict == null) {
					//dict = new Dictionary<string, AutomatedOrder> (orders.Count ());
					dict = new Dictionary<string, AutomatedOrder> ();
				} else if (dict.ContainsKey (id)) {
					return;
				}


				dict.Add (id, order);



			}


		}


		public void UpdateAndSave (AutomatedOrder order /*, string account*/)
		{

			lock (lockobj) {
				if (order == null) {
					return;
				}

				string id = order.Bot_ID;
				Logging.WriteLog ("Synccache : " + (id ?? "null"));

				Dictionary<string, AutomatedOrder> dict = this.SequenceCache;
				if (dict == null) {
					//dict = new Dictionary<string, AutomatedOrder> (orders.Count ());
					dict = new Dictionary<string, AutomatedOrder> ();
				} else if (dict.ContainsKey (id)) {
					return;
				}


				dict.Add (id, order);

				IEnumerable<AutomatedOrder> offers = this.SequenceCache?.Values;

				var settingsPath = FileHelper.GetSettingsPath (Account + settingsFileName);

				//ConfStruct confstruct = new ConfStruct( orderDict.Values );
				ConfStruct confstruct = new ConfStruct (offers);

				string conf = DynamicJson.Serialize (confstruct);

				FileHelper.SaveConfig (settingsPath, conf);

			}


		}



		public void SyncOrdersCache (string account, CancellationToken token)
		{
			if (account == null) {
				return;
			}

			NetworkInterface networkInterface = NetworkController.CurrentInterface;

			if (networkInterface == null) {
				return;
			}

			Task<IEnumerable<Response<AccountOffersResult>>> task = AccountOffers.GetFullOfferList (account, networkInterface);


			if (task == null) {
				return;
			}

			task.Wait (token);


			IEnumerable<Response<AccountOffersResult>> responses = task.Result;

			Dictionary<string, AutomatedOrder> cached = Load (account);





			foreach (Response<AccountOffersResult> resp in responses) {

				var offs = resp?.result?.offers;
				if (offs == null) {

					if (OnOrderCacheEvent != null) {
						OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
							GetOrder = null,
							GetSuccess = false,
							Message = "No offers in server response"
						};

						OnOrderCacheEvent.Invoke (this, cachedEventArgs);
					}

					continue;
				}

				//List<AutomatedOrder> auts_list = new List<AutomatedOrder> ();

				int ordercount = 0;
				foreach (Offer o in offs) {


					string id = o.Account + o.Sequence.ToString ();

					if (cached != null && cached.ContainsKey (id)) {
						if (OnOrderCacheEvent != null) {
							string message = "Order with sequence " + o.Sequence.ToString () + " already in cache\n";
							OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs () {
								GetOrder = o,
								GetSuccess = true,
								Message = message
							};


							OnOrderCacheEvent.Invoke (this, cachedEventArgs);
						}
						continue;
					} else {

						Task<Response<string>> seqTask = tx.GetTxFromAccountAndSequenceDataAPI (o.Account, o.Sequence);
						if (seqTask == null) {


							if (OnOrderCacheEvent != null) {
								string message = "Failed to retrive order with sequence " + o.Sequence.ToString() + " possible network issues\n";
								OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
									GetOrder = o,
									GetSuccess = false,
									Message = message
								};


								OnOrderCacheEvent.Invoke (this, cachedEventArgs);
							}


							Thread.Sleep (5000);
							continue;
						}
						seqTask.Wait (token);


						Response<string> response = seqTask.Result;
						if (response == null) {

							if (OnOrderCacheEvent != null) {
								string message = "Failed to retrive order with sequence " + o.Sequence.ToString () + " response is null\n" ;
								OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
									GetOrder = o,
									GetSuccess = false,
									Message = message
								};


								OnOrderCacheEvent.Invoke (this, cachedEventArgs);
							}

							Thread.Sleep (5000);
							continue;
						}
						string s = response.result;
						var txstruct = response.transaction;
						//txstruct.tx.
						AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (txstruct.tx);

						if (ao == null) {
							if (OnOrderCacheEvent != null) {
								string message = "Failed to retrive order with sequence " + o.Sequence.ToString () + " unable to reconstruct order from tx\n";
								OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
									GetOrder = o,
									GetSuccess = false,
									Message = message
								};


								OnOrderCacheEvent.Invoke (this, cachedEventArgs);
							}
							continue;
						}


						if (OnOrderCacheEvent != null) {
							string message = "Caching order with sequence " + o.Sequence.ToString () + "\n";
							OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
								GetOrder = o,
								GetSuccess = true,
								Message = message
							};


							OnOrderCacheEvent.Invoke (this, cachedEventArgs);
						}

						UpdateOrdersCache (ao /*, account */);
					}

					if (ordercount++ % 5 == 4) {
						if (OnOrderCacheEvent != null) {
							OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
								GetOrder = null,
								GetSuccess = true,
								Message = "Saving cache to file\n"
							};

							OnOrderCacheEvent.Invoke (this, cachedEventArgs);
						}

						this.Save ();
					}

				}

				if (OnOrderCacheEvent != null && ordercount % 5 != 0) {
					OrderCachedEventArgs cachedEventArgs = new OrderCachedEventArgs {
						GetOrder = null,
						GetSuccess = true,
						Message = "Saving cache to file\n"
					};

					OnOrderCacheEvent.Invoke (this, cachedEventArgs);
				}

				this.Save ();

				/*
				if (!auts_list.Any ()) {
					continue;
				}*/



			}





		}


		public Dictionary<String, AutomatedOrder> SequenceCache;

		public static Dictionary<String, AutomatedOrder> Load (string account)
		{


			var settingsPath = FileHelper.GetSettingsPath (account + settingsFileName);
			string str = null;

			lock (lockobj) {
				str = FileHelper.GetJsonConf (settingsPath);



				if (str == null) {
					return null;
				}
				ConfStruct jsconf = null;
				try {
					jsconf = DynamicJson.Parse (str);

				} catch (Exception e) {
					Logging.WriteLog (e.Message + e.StackTrace);
					return null;
				}

				if (jsconf == null) {
					return null;
				}

				AutomatedOrder [] ords = jsconf.Orders;

				Dictionary<String, AutomatedOrder> orderDict = new Dictionary<string, AutomatedOrder> (ords.Length);

				//orderDict.Clear ();
				foreach (AutomatedOrder order in ords) {
					string key = order.Bot_ID;

					if (key != null) {
						//orderDict.TryAdd (key, order);
						orderDict.Add (key, order);
					}
				}

				return orderDict;
			}


		}




		private class ConfStruct
		{
			public ConfStruct (IEnumerable<AutomatedOrder> ords)
			{


				if (ords == null) {
					return;
				}
				int count = ords.Count ();

				var it = ords.GetEnumerator ();



				this.Orders = new AutomatedOrder [count];

				for (int i = 0; i < count; i++) {
					it.MoveNext ();
					Orders [i] = it.Current;

				}

			}

			public ConfStruct ()
			{

			}


			public AutomatedOrder [] Orders {
				get;
				set;
			}

			/*
			public int latestLedger {
				get;
				set;
			}
			*/

		}


		public static void DeleteSettingsFile (string account)
		{
			var settingsPath = FileHelper.GetSettingsPath (account + settingsFileName);
			lock (lockobj) {

				if (File.Exists (settingsPath)) {
					File.Delete (settingsPath);
				}
			}

		}

		public void Save ()
		{
			lock (lockobj) {

				IEnumerable<AutomatedOrder> offers = this.SequenceCache?.Values;
				if (offers == null) {
					return;
				}
				var settingsPath = FileHelper.GetSettingsPath (Account + settingsFileName);

				//ConfStruct confstruct = new ConfStruct( orderDict.Values );
				ConfStruct confstruct = new ConfStruct (offers);

				string conf = DynamicJson.Serialize (confstruct);

				FileHelper.SaveConfig (settingsPath, conf);
			}
		}

		private static readonly object lockobj = new object ();

		public event EventHandler<OrderCachedEventArgs> OnOrderCacheEvent;

		public const string settingsFileName = "OrderManagementBot.jsn";

	}



	public class OrderCachedEventArgs : EventArgs
	{

		public bool GetSuccess {
			get;
			set;
		}

		public Offer GetOrder {
			get;
			set;
		}

		public string Message {
			get;
			set;
		}

	}

}
