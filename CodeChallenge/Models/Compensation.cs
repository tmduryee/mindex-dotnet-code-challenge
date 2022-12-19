using System;

namespace CodeChallenge.Models
{
	// Requires primary key (CompensationId) to track in DB
	public class Compensation
	{
		public int CompensationId { get; set; }
		public Employee Employee { get; set; }
		public double Salary { get; set; }
		public DateTime EffectiveDate { get; set; }
	}
}
