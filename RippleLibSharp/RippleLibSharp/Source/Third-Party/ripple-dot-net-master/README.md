# Ripple.NET [![Build status](https://ci.appveyor.com/api/projects/status/w1jnx38oreo35a6b/branch/master?svg=true)](https://ci.appveyor.com/project/sublimator/ripple-dot-net)

The purpose of this code is primarily for signing transactions, and while the
code can do a bit more than what is shown here, at this stage, it may change
without warning. Only the Transaction signing api shown below has any
guarantees. While this is currently a low priority project, feel free to register
your interest at support@ripple.com

## Requirements

* Visual Studio 2015 or equivalent compiler with c#6 support
* .NET framework 4.5.0+

## Transaction signing

Signing is done with either ecdsa/rfc6979 or ed25519. See [ripple-keypairs](https://github.com/ripple/ripple-keypairs) for how to generate a seed/secret, encoded in base58. 

```c#

// using Ripple.TxSigning
// using Newtonsoft.Json.Linq;

var secret = "sEd7rBGm5kxzauRTAV2hbsNz7N45X91";
var unsignedTxJson = @"{
    'Account': 'rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN',
    'Amount': '1000',
    'Destination': 'rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh',
    'Fee': '10',
    'Flags': 2147483648,
    'Sequence': 1,
    'TransactionType' : 'Payment'
}";

var signed = TxSigner.SignJson(JObject.Parse(unsignedTxJson), secret);

Console.WriteLine(signed.Hash);
Console.WriteLine(signed.TxJson);
Console.WriteLine(signed.TxBlob);

// A8A9C869671D35A18DFB69AFB7741062DF43F73C8A5942AD94EE58ED31477AC6
// {
//   "Account": "rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN",
//   "Amount": "1000",
//   "Destination": "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh",
//   "Fee": "10",
//   "Flags": 2147483648,
//   "Sequence": 1,
//   "TransactionType": "Payment",
//   "SigningPubKey": "EDD3993CDC6647896C455F136648B7750723B011475547AF60691AA3D7438E021D",
//   "TxnSignature": "C3646313B08EED6AF4392261A31B961F10C66CB733DB7F6CD9EAB079857834C8B0334270A2C037E63CDCCC1932E0832882B7B7066ECD2FAEDEB4A83DF8AE6303"
// }
// 120000228000000024000000016140000000000003E868400000000000000A7321EDD3993CDC6647896C455F136648B7750723B011475547AF60691AA3D7438E021D7440C3646313B08EED6AF4392261A31B961F10C66CB733DB7F6CD9EAB079857834C8B0334270A2C037E63CDCCC1932E0832882B7B7066ECD2FAEDEB4A83DF8AE63038114C0A5ABEF242802EFED4B041E8F2D4A8CC86AE3D18314B5F762798A53D543A014CAF8B297CFF8F2F937E8

```
