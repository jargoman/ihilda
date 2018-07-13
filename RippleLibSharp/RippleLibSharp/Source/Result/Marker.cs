using System;

namespace RippleLibSharp.Result
{
	public class Marker
	{
		public Marker ()
		{
		}

		public Marker (string s) {
			this._Marker_String = s;
		}

		private string _Marker_String {
			get;
			set;
		}

#pragma warning disable IDE1006 // Naming Styles
		public int? ledger {

			get;
			set;
		}


		public int? seq {
			get {
				return _sequence;
			}
			set {
				this._sequence = value;
			}
		}
#pragma warning restore IDE1006 // Naming Styles


		public object GetObject () {
			
			if (_Marker_String != null) {
				return this._Marker_String;
			}

			return this;

		}



		// likely not needed 
#pragma warning disable IDE1006 // Naming Styles
		public int? sequence {


			get {
				return _sequence;
			}
			set {
				this._sequence = value;
			}

		}
#pragma warning restore IDE1006 // Naming Styles


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private int? _sequence = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
	}
}

