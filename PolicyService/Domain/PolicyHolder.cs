namespace PolicyService.Domain;

public class PolicyHolder
{
    public PolicyHolder(string firstName, string lastName, string pesel, Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        Pesel = pesel;
        Address = address;
    }

    protected PolicyHolder()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Pesel = string.Empty;
        Address = Address.Of(string.Empty, string.Empty, string.Empty, string.Empty);
    } //NH required

    public virtual string FirstName { get; protected set; }
    public virtual string LastName { get; protected set; }
    public virtual string Pesel { get; protected set; }
    public virtual Address Address { get; protected set; }
}
