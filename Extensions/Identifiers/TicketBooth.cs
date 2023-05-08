namespace Extensions
{
	public class TicketBooth
	{
		private static ulong id = 0;

		private ulong lastTicketID = 0;

		public ulong ID { get; private set; }

		public TicketBooth() => ID = id++;

		public Ticket GetTicket() => new Ticket(++lastTicketID);

		public bool IsLast(Ticket ticket) => lastTicketID == ticket.ID;

		public override string ToString() => $"Ticket[{ID}]";

		public override bool Equals(object obj) => obj is TicketBooth ticket &&
						ID == ticket.ID;

		public override int GetHashCode() => 1213502048 + ID.GetHashCode();

		public class Ticket
		{
			internal ulong ID;

			internal Ticket(ulong id) => ID = id;
		}
	}
}