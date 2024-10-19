using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Person
{
    public string Name { get; set; }
    public bool IsInfected { get; set; }
    public bool IsHealed { get; set; }
    public List<Person> Friends { get; set; }  = new List<Person>();    
    public void AddFriend(Person friend)
    {
        Friends.Add(friend);
    }

}
class DiseaseModel
{
    private readonly double _p1;
    private readonly double _p2;
    public List<Person> People { get; set; } =  new List<Person>();

    public DiseaseModel(double p1, double p2)
    {
        _p1 = p1;
        _p2 = p2;
    }

    public async Task SpreadDiseaseAsync()
    {
        foreach(var person in People)
        {
            if(person.IsInfected && !person.IsHealed)
            {
            
                foreach (var friend in person.Friends)
                {
                    if (!friend.IsInfected && new Random().NextDouble() < _p1) //Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
                    {
                        friend.IsInfected = true; 
                    }
                }
            
            }
            if(person.IsInfected && new Random().NextDouble() < _p2) 
            { 
                person.IsHealed = true;

            }    
        }
        await Task.Delay(100); 
    }
    
    public List<string> GetNonInfected()
    {
        return People.Where(p => !p.IsInfected).Select(p=>p.Name).ToList();
    }
    public List <string> GetHealed() 
    {
        return People.Where(p=> p.IsHealed).Select(p=>p.Name).ToList();
    }
    public List<string> GetHealedWithNonHealedFriends()
    {
        return People.Where(p => p.IsHealed && p.Friends.All(f => !f.IsHealed)).Select(p => p.Name).ToList();
    }
    public List<string> GetNonInfectedWithInfectedFriends()
    {
        return People.Where(p => !p.IsInfected && p.Friends.All(f => f.IsInfected)).Select(p => p.Name).ToList();
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        double p1 = 0.3;
        double p2 = 0.5;

        var model = new DiseaseModel(p1, p2);

        var alice = new Person { Name = "Alice", IsInfected = true }; 
        var bob = new Person { Name = "Bob" };
        var charlie = new Person { Name = "Charlie" };

        alice.AddFriend(bob);
        bob.AddFriend(alice);
        bob.AddFriend(charlie);
        charlie.AddFriend(bob);

        model.People.AddRange(new[] { alice, bob, charlie });

        for (int day = 0; day < 10; day++)
        {
            await model.SpreadDiseaseAsync();
            Console.WriteLine($"Day {day + 1}:");
            Console.WriteLine("Not infected: " + string.Join(", ", model.GetNonInfected()));
            Console.WriteLine("Healed: " + string.Join(", ", model.GetHealed()));
            Console.WriteLine("Healed people with unhealed surroundings: " + string.Join(", ", model.GetHealedWithNonHealedFriends()));
            Console.WriteLine("Non-infected people with infected surroundings: " + string.Join(", ", model.GetNonInfectedWithInfectedFriends()));
            Console.WriteLine();
        }
        await File.WriteAllTextAsync("results.txt", "Simulation completed.");
    }
}