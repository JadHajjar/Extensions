namespace Extensions;

public class TicketBooth
{
	private static ulong id = 0;

	private ulong lastTicketID = 0;

	public ulong ID { get; private set; }

	public TicketBooth()
	{
		ID = id++;
	}

	public Ticket GetTicket()
	{
		return new Ticket(++lastTicketID);
	}

	public bool IsLast(Ticket ticket)
	{
		return lastTicketID == ticket.ID;
	}

	public override string ToString()
	{
		return $"Ticket[{ID}]";
	}

	public override bool Equals(object obj)
	{
		return obj is TicketBooth ticket &&
					ID == ticket.ID;
	}

	public override int GetHashCode()
	{
		return 1213502048 + ID.GetHashCode();
	}

	public class Ticket
	{
		internal ulong ID;

		internal Ticket(ulong id)
		{
			ID = id;
		}
	}
}