
namespace Helix.Utility
{
	using System.Text.RegularExpressions;

	public sealed class Comparer
	{
		public static bool IsBetween(int testArg, int ceiling, int floor)
		{
			return (ceiling >= testArg && floor <= testArg);
		}

		public static bool IsGreater(int testArg, int ceiling)
		{
			return (testArg > ceiling);
		}

		public static bool IsLessThan(int testArg, int floor)
		{
			return (testArg < floor);
		}


		public static bool IsValidUrl(string url)
		{
			return new Regex(@"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?").IsMatch(url);
		}

		public static bool IsValidPhone(string phone)
		{
			return new Regex(@"^(?:\([2-9]\d{2}\)\ ?|[2-9]\d{2}(?:\-?|\ ?))[2-9]\d{2}[- ]?\d{4}$").IsMatch(phone);
		}

		public static bool IsValidDate(string date)
		{
			return
				new Regex(
					@"(?n:^(?=\d)((?<month>(0?[13578])|1[02]|(0?[469]|11)(?!.31)|0
									?2(?(.29)(?=.29.((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][
									26])|(16|[2468][048]|[3579][26])00))|(?!.3[01])))(?<sep>[-./
									])(?<day>0?[1-9]|[12]\d|3[01])\k<sep>(?<year>(1[6-9]|[2-9]\d
									)\d{2})(?(?=\x20\d)\x20|$))?(?<time>((0?[1-9]|1[012])(:[0-5]
									\d){0,2}(?i:\x20[AP]M))|([01]\d|2[0-3])(:[0-5]\d){1,2})?$)")
					.IsMatch(date);
		}

		public static bool IsValidSsn(string ssn)
		{
			return
				new Regex(
					@"^(?!000)(?!666)(?&lt;SSN3&gt;[0-6]\d{2}|7(?:[0-6]\d|7[012]))([- ]?)(?!00)(?&lt;SSN2&gt;\d\d)\1(?!0000)(?&lt;SSN4&gt;\d{4})$")
					.IsMatch(ssn);
		}

		public static bool IsStrongPassword(string password)
		{
			return new Regex(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).{4,8}$").IsMatch(password);
		}

		public static bool IsValidCurrency(string money)
		{
			return
				new Regex(
					@"^(?!\u00a2)  #Don't allow cent symbol
									\p{Sc}?     #optional unicode currency symbols
									(?!0,?\d)   #don't allow leading zero if 1 or more unit
									 (\d{1,3}    # 1 to 3 digits
									(\,\d{3})*  # if the is a comma it must be followed by 3 digits
									|(\d+))      # more than 3 digit with no comma separator
									(\.\d{2})?$  # option cents")
					.IsMatch(money);
		}

		public static bool IsWellFormedXml(string xml)
		{
			return new Regex("<(\\w+)(\\s(\\w*=\".*?\")?)*((/>)|((/*?)>.*?</\\1>))").IsMatch(xml);
		}

		public static bool IsValidZip(string zip)
		{
			return
				new Regex(@"^((\d{5}-\d{4})|(\d{5})|([AaBbCcEeGgHhJjKkLlMmNnPpRrSsTtVvXxYy]\d[A-Za-z]\s?\d[A-Za-z]\d))$")
					.IsMatch(zip);
		}

		public static bool IsValid(string testValue, string testRegex)
		{
			return new Regex(testRegex).IsMatch(testValue);
		}

	}
}
