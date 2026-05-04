using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class TriviaRepository : ITriviaRepository
{
    private readonly MovieAppDbContext context;

    public TriviaRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<TriviaQuestion>> GetAllAsync(CancellationToken ct = default)
    {
        await this.EnsureSeededAsync(ct);
        return await this.context.TriviaQuestions.ToListAsync(ct);
    }

    public async Task<TriviaQuestion?> GetRandomAsync(CancellationToken ct = default)
    {
        await this.EnsureSeededAsync(ct);
        return await this.context.TriviaQuestions
            .OrderBy(r => Guid.NewGuid())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string categoryName, CancellationToken ct = default)
    {
        await this.EnsureSeededAsync(ct);
        return await this.context.TriviaQuestions
            .Where(q => q.Category == categoryName)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int questionCount = ITriviaRepository.DefaultQuestionCount, CancellationToken ct = default)
    {
        await this.EnsureSeededAsync(ct);
        return await this.context.TriviaQuestions
            .Where(q => q.MovieId == movieId)
            .Take(questionCount)
            .ToListAsync(ct);
    }

    private async Task EnsureSeededAsync(CancellationToken ct)
    {
        var dummyQuestions = await this.context.TriviaQuestions
            .Where(q => q.QuestionText.Contains("Sample trivia question"))
            .ToListAsync(ct);
            
        if (dummyQuestions.Any())
        {
            this.context.TriviaQuestions.RemoveRange(dummyQuestions);
            await this.context.SaveChangesAsync(ct);
        }

        if (await this.context.TriviaQuestions.AnyAsync(ct))
        {
            return;
        }

        var trivia = new List<TriviaQuestion>
        {
            new TriviaQuestion { QuestionText = "Which actor played Iron Man in the Marvel Cinematic Universe?", Category = "Actors", OptionA = "Chris Evans", OptionB = "Robert Downey Jr.", OptionC = "Mark Ruffalo", OptionD = "Chris Hemsworth", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played the Joker in the 2019 film Joker?", Category = "Actors", OptionA = "Heath Ledger", OptionB = "Jack Nicholson", OptionC = "Joaquin Phoenix", OptionD = "Jared Leto", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which actress starred as Katniss Everdeen in The Hunger Games?", Category = "Actors", OptionA = "Emma Watson", OptionB = "Jennifer Lawrence", OptionC = "Shailene Woodley", OptionD = "Kristen Stewart", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played Forrest Gump in the 1994 film?", Category = "Actors", OptionA = "Tom Hanks", OptionB = "Tom Cruise", OptionC = "Kevin Costner", OptionD = "Denzel Washington", CorrectOption = 'A' },
            new TriviaQuestion { QuestionText = "Which actor portrayed Jack Sparrow in Pirates of the Caribbean?", Category = "Actors", OptionA = "Orlando Bloom", OptionB = "Johnny Depp", OptionC = "Geoffrey Rush", OptionD = "Javier Bardem", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played the lead role in The Revenant (2015)?", Category = "Actors", OptionA = "Matt Damon", OptionB = "Brad Pitt", OptionC = "Leonardo DiCaprio", OptionD = "Christian Bale", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which actress won an Oscar for her role in La La Land?", Category = "Actors", OptionA = "Emma Stone", OptionB = "Natalie Portman", OptionC = "Amy Adams", OptionD = "Cate Blanchett", CorrectOption = 'A' },
            new TriviaQuestion { QuestionText = "Who played Wolverine in the X-Men film series?", Category = "Actors", OptionA = "Liam Neeson", OptionB = "Hugh Jackman", OptionC = "Russell Crowe", OptionD = "Gerard Butler", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which actor starred as Neo in The Matrix?", Category = "Actors", OptionA = "Will Smith", OptionB = "Keanu Reeves", OptionC = "Laurence Fishburne", OptionD = "Hugo Weaving", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played Hermione Granger in the Harry Potter series?", Category = "Actors", OptionA = "Emma Watson", OptionB = "Helena Bonham Carter", OptionC = "Evanna Lynch", OptionD = "Bonnie Wright", CorrectOption = 'A' },
            new TriviaQuestion { QuestionText = "Which actor played T Challa in Black Panther?", Category = "Actors", OptionA = "Idris Elba", OptionB = "Michael B. Jordan", OptionC = "Chadwick Boseman", OptionD = "Lupita Nyongo", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who starred as Maximus in Gladiator (2000)?", Category = "Actors", OptionA = "Russell Crowe", OptionB = "Mel Gibson", OptionC = "Brad Pitt", OptionD = "Antonio Banderas", CorrectOption = 'A' },
            new TriviaQuestion { QuestionText = "Which actress played Clarice Starling in The Silence of the Lambs?", Category = "Actors", OptionA = "Sigourney Weaver", OptionB = "Jodie Foster", OptionC = "Meryl Streep", OptionD = "Susan Sarandon", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played the Terminator in the original 1984 film?", Category = "Actors", OptionA = "Sylvester Stallone", OptionB = "Jean-Claude Van Damme", OptionC = "Arnold Schwarzenegger", OptionD = "Dolph Lundgren", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which actor played Gandalf in The Lord of the Rings trilogy?", Category = "Actors", OptionA = "Patrick Stewart", OptionB = "Ian McKellen", OptionC = "Christopher Lee", OptionD = "Anthony Hopkins", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who portrayed Steve Rogers in the Marvel Cinematic Universe?", Category = "Actors", OptionA = "Chris Hemsworth", OptionB = "Chris Pratt", OptionC = "Chris Evans", OptionD = "Jeremy Renner", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which actress played Mia in La La Land?", Category = "Actors", OptionA = "Natalie Portman", OptionB = "Emma Stone", OptionC = "Anne Hathaway", OptionD = "Scarlett Johansson", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who played Han Solo in the original Star Wars trilogy?", Category = "Actors", OptionA = "Mark Hamill", OptionB = "Harrison Ford", OptionC = "Carrie Fisher", OptionD = "Billy Dee Williams", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which actor played Tyler Durden in Fight Club?", Category = "Actors", OptionA = "Edward Norton", OptionB = "Brad Pitt", OptionC = "Jared Leto", OptionD = "Matt Damon", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who starred as Dom Toretto in the Fast and Furious franchise?", Category = "Actors", OptionA = "Dwayne Johnson", OptionB = "Paul Walker", OptionC = "Vin Diesel", OptionD = "Tyrese Gibson", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who directed Inception (2010)?", Category = "Directors", OptionA = "Steven Spielberg", OptionB = "Christopher Nolan", OptionC = "James Cameron", OptionD = "Ridley Scott", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which director is known for the films Pulp Fiction and Kill Bill?", Category = "Directors", OptionA = "Martin Scorsese", OptionB = "Quentin Tarantino", OptionC = "David Fincher", OptionD = "Coen Brothers", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who directed Titanic (1997)?", Category = "Directors", OptionA = "Steven Spielberg", OptionB = "Ridley Scott", OptionC = "James Cameron", OptionD = "Ron Howard", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director made Schindlers List?", Category = "Directors", OptionA = "Francis Ford Coppola", OptionB = "Martin Scorsese", OptionC = "Steven Spielberg", OptionD = "Stanley Kubrick", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who directed The Godfather (1972)?", Category = "Directors", OptionA = "Martin Scorsese", OptionB = "Francis Ford Coppola", OptionC = "Brian De Palma", OptionD = "Sidney Lumet", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which director is behind the Toy Story franchise?", Category = "Directors", OptionA = "Brad Bird", OptionB = "Andrew Stanton", OptionC = "John Lasseter", OptionD = "Pete Docter", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who directed The Dark Knight (2008)?", Category = "Directors", OptionA = "Zack Snyder", OptionB = "Tim Burton", OptionC = "Christopher Nolan", OptionD = "Joel Schumacher", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director made Get Out (2017)?", Category = "Directors", OptionA = "Spike Lee", OptionB = "Jordan Peele", OptionC = "Ryan Coogler", OptionD = "Ava DuVernay", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who directed Parasite (2019)?", Category = "Directors", OptionA = "Park Chan-wook", OptionB = "Wong Kar-wai", OptionC = "Bong Joon-ho", OptionD = "Kim Jee-woon", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director is known for the Harry Potter film series (first film)?", Category = "Directors", OptionA = "Alfonso Cuaron", OptionB = "Mike Newell", OptionC = "David Yates", OptionD = "Chris Columbus", CorrectOption = 'D' },
            new TriviaQuestion { QuestionText = "Who directed Avatar (2009)?", Category = "Directors", OptionA = "Steven Spielberg", OptionB = "Peter Jackson", OptionC = "James Cameron", OptionD = "Michael Bay", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director made The Silence of the Lambs?", Category = "Directors", OptionA = "David Fincher", OptionB = "Jonathan Demme", OptionC = "Ridley Scott", OptionD = "Brian De Palma", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who directed Interstellar (2014)?", Category = "Directors", OptionA = "Denis Villeneuve", OptionB = "Christopher Nolan", OptionC = "Alfonso Cuaron", OptionD = "Ridley Scott", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which director is known for making Psycho (1960)?", Category = "Directors", OptionA = "Stanley Kubrick", OptionB = "Orson Welles", OptionC = "Alfred Hitchcock", OptionD = "Billy Wilder", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who directed The Lord of the Rings trilogy?", Category = "Directors", OptionA = "Guillermo del Toro", OptionB = "Ridley Scott", OptionC = "Peter Jackson", OptionD = "Sam Raimi", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director made Whiplash (2014)?", Category = "Directors", OptionA = "Damien Chazelle", OptionB = "Derek Cianfrance", OptionC = "Bennett Miller", OptionD = "Tom McCarthy", CorrectOption = 'A' },
            new TriviaQuestion { QuestionText = "Who directed Blade Runner (1982)?", Category = "Directors", OptionA = "James Cameron", OptionB = "Ridley Scott", OptionC = "Steven Spielberg", OptionD = "Terry Gilliam", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which director made The Grand Budapest Hotel?", Category = "Directors", OptionA = "Michel Gondry", OptionB = "Spike Jonze", OptionC = "Wes Anderson", OptionD = "Sofia Coppola", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who directed Goodfellas (1990)?", Category = "Directors", OptionA = "Francis Ford Coppola", OptionB = "Brian De Palma", OptionC = "Martin Scorsese", OptionD = "Michael Mann", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which director made Mad Max: Fury Road (2015)?", Category = "Directors", OptionA = "Ridley Scott", OptionB = "George Miller", OptionC = "James Cameron", OptionD = "Zack Snyder", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film contains the quote: \"Here is looking at you, kid\"?", Category = "Movie Quotes", OptionA = "Gone with the Wind", OptionB = "Casablanca", OptionC = "Sunset Boulevard", OptionD = "Rebecca", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which movie does a character say \"I am your father\"?", Category = "Movie Quotes", OptionA = "Star Wars: A New Hope", OptionB = "Star Wars: Return of the Jedi", OptionC = "Star Wars: The Empire Strikes Back", OptionD = "Star Wars: Revenge of the Sith", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features the line: \"You cannot handle the truth\"?", Category = "Movie Quotes", OptionA = "The Firm", OptionB = "A Few Good Men", OptionC = "Philadelphia", OptionD = "JFK", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which movie does someone say \"Why so serious?\"?", Category = "Movie Quotes", OptionA = "Batman Begins", OptionB = "Batman v Superman", OptionC = "The Dark Knight", OptionD = "Joker", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film contains the quote: \"To infinity and beyond\"?", Category = "Movie Quotes", OptionA = "Interstellar", OptionB = "Toy Story", OptionC = "The Hitchhikers Guide to the Galaxy", OptionD = "Wall-E", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which movie is the line \"Life is like a box of chocolates\" spoken?", Category = "Movie Quotes", OptionA = "Big", OptionB = "Cast Away", OptionC = "Forrest Gump", OptionD = "Philadelphia", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features the quote: \"I see dead people\"?", Category = "Movie Quotes", OptionA = "Poltergeist", OptionB = "The Others", OptionC = "The Sixth Sense", OptionD = "Insidious", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In which movie does a character say \"Just keep swimming\"?", Category = "Movie Quotes", OptionA = "Shark Tale", OptionB = "Finding Nemo", OptionC = "The Little Mermaid", OptionD = "Moana", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film contains the line: \"You is kind, you is smart, you is important\"?", Category = "Movie Quotes", OptionA = "Precious", OptionB = "Selma", OptionC = "The Help", OptionD = "Hidden Figures", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In which movie is the quote \"Why so serious?\" first said?", Category = "Movie Quotes", OptionA = "Batman Forever", OptionB = "Batman and Robin", OptionC = "The Dark Knight", OptionD = "Batman Begins", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features: \"Get busy living or get busy dying\"?", Category = "Movie Quotes", OptionA = "The Green Mile", OptionB = "The Shawshank Redemption", OptionC = "Cool Hand Luke", OptionD = "Papillon", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which movie does someone say \"I feel the need, the need for speed\"?", Category = "Movie Quotes", OptionA = "Days of Thunder", OptionB = "Top Gun", OptionC = "Talladega Nights", OptionD = "Rush", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film contains: \"E.T. phone home\"?", Category = "Movie Quotes", OptionA = "Close Encounters of the Third Kind", OptionB = "Contact", OptionC = "E.T. the Extra-Terrestrial", OptionD = "Alien", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In which movie is \"You talking to me?\" famously said?", Category = "Movie Quotes", OptionA = "Raging Bull", OptionB = "Mean Streets", OptionC = "Taxi Driver", OptionD = "GoodFellas", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features the quote: \"Hasta la vista, baby\"?", Category = "Movie Quotes", OptionA = "The Terminator", OptionB = "Predator", OptionC = "Terminator 2: Judgment Day", OptionD = "Total Recall", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In which movie does a character say \"My precious\"?", Category = "Movie Quotes", OptionA = "The Hobbit", OptionB = "The Lord of the Rings: The Fellowship of the Ring", OptionC = "The Lord of the Rings: The Two Towers", OptionD = "The Lord of the Rings: The Return of the King", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film contains: \"With great power comes great responsibility\"?", Category = "Movie Quotes", OptionA = "Batman Begins", OptionB = "Spider-Man", OptionC = "Iron Man", OptionD = "Superman", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which movie is \"I am Groot\" said?", Category = "Movie Quotes", OptionA = "Thor", OptionB = "Avengers: Infinity War", OptionC = "Guardians of the Galaxy", OptionD = "Avengers: Endgame", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features: \"You had me at hello\"?", Category = "Movie Quotes", OptionA = "Pretty Woman", OptionB = "Notting Hill", OptionC = "Jerry Maguire", OptionD = "Sleepless in Seattle", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In which movie does someone say \"They may take our lives, but they will never take our freedom\"?", Category = "Movie Quotes", OptionA = "Rob Roy", OptionB = "The Patriot", OptionC = "Braveheart", OptionD = "Gladiator", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film won the first Academy Award for Best Picture?", Category = "Oscars and Awards", OptionA = "It Happened One Night", OptionB = "Wings", OptionC = "All Quiet on the Western Front", OptionD = "Cimarron", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "How many Oscars did Titanic (1997) win?", Category = "Oscars and Awards", OptionA = "9", OptionB = "11", OptionC = "14", OptionD = "7", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which actress has won the most Academy Awards for acting?", Category = "Oscars and Awards", OptionA = "Meryl Streep", OptionB = "Katharine Hepburn", OptionC = "Bette Davis", OptionD = "Ingrid Bergman", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film won Best Picture at the 2020 Academy Awards?", Category = "Oscars and Awards", OptionA = "Once Upon a Time in Hollywood", OptionB = "1917", OptionC = "Joker", OptionD = "Parasite", CorrectOption = 'D' },
            new TriviaQuestion { QuestionText = "Who was the first African American to win the Best Actor Oscar?", Category = "Oscars and Awards", OptionA = "Denzel Washington", OptionB = "Sidney Poitier", OptionC = "Jamie Foxx", OptionD = "Forest Whitaker", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film holds the record for most Oscar wins?", Category = "Oscars and Awards", OptionA = "Titanic", OptionB = "Ben-Hur", OptionC = "Lord of the Rings: Return of the King", OptionD = "All three are tied", CorrectOption = 'D' },
            new TriviaQuestion { QuestionText = "Who won Best Director for The Silence of the Lambs?", Category = "Oscars and Awards", OptionA = "David Fincher", OptionB = "Jonathan Demme", OptionC = "Ridley Scott", OptionD = "Martin Scorsese", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which animated film won the first Oscar for Best Animated Feature?", Category = "Oscars and Awards", OptionA = "Monsters Inc", OptionB = "Shrek", OptionC = "Ice Age", OptionD = "Jimmy Neutron", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Who won Best Actress for her role in Monster (2003)?", Category = "Oscars and Awards", OptionA = "Naomi Watts", OptionB = "Nicole Kidman", OptionC = "Charlize Theron", OptionD = "Hilary Swank", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film won Best Picture at the 2017 Academy Awards after a mix-up?", Category = "Oscars and Awards", OptionA = "La La Land", OptionB = "Moonlight", OptionC = "Manchester by the Sea", OptionD = "Hidden Figures", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "How many times has Meryl Streep been nominated for an Academy Award?", Category = "Oscars and Awards", OptionA = "17", OptionB = "21", OptionC = "25", OptionD = "14", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which director won their first Oscar for The Departed (2006)?", Category = "Oscars and Awards", OptionA = "Francis Ford Coppola", OptionB = "Martin Scorsese", OptionC = "Clint Eastwood", OptionD = "Ridley Scott", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film won Best Picture at the 92nd Academy Awards?", Category = "Oscars and Awards", OptionA = "1917", OptionB = "Ford v Ferrari", OptionC = "Parasite", OptionD = "Joker", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who won Best Actor for playing Ray Charles in Ray (2004)?", Category = "Oscars and Awards", OptionA = "Will Smith", OptionB = "Denzel Washington", OptionC = "Jamie Foxx", OptionD = "Don Cheadle", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which country does Parasite represent?", Category = "Oscars and Awards", OptionA = "Japan", OptionB = "China", OptionC = "South Korea", OptionD = "Taiwan", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who won Best Supporting Actor for Pulp Fiction?", Category = "Oscars and Awards", OptionA = "John Travolta", OptionB = "Samuel L. Jackson", OptionC = "Martin Landau", OptionD = "Ed Wood", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film won Best Picture in 2010?", Category = "Oscars and Awards", OptionA = "Avatar", OptionB = "Inglourious Basterds", OptionC = "The Hurt Locker", OptionD = "Up in the Air", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Who won Best Actress for La La Land?", Category = "Oscars and Awards", OptionA = "Natalie Portman", OptionB = "Cate Blanchett", OptionC = "Emma Stone", OptionD = "Meryl Streep", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which actor won Best Actor for The Revenant (2015)?", Category = "Oscars and Awards", OptionA = "Matt Damon", OptionB = "Michael Fassbender", OptionC = "Leonardo DiCaprio", OptionD = "Bryan Cranston", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "How many Oscar categories did The Lord of the Rings: The Return of the King win?", Category = "Oscars and Awards", OptionA = "9", OptionB = "11", OptionC = "13", OptionD = "7", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "What year was the first Star Wars film released?", Category = "General Movie Trivia", OptionA = "1975", OptionB = "1977", OptionC = "1979", OptionD = "1980", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which studio produced the Lion King (1994)?", Category = "General Movie Trivia", OptionA = "Pixar", OptionB = "DreamWorks", OptionC = "Walt Disney Animation", OptionD = "Universal", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "What is the highest-grossing film of all time (not adjusted for inflation)?", Category = "General Movie Trivia", OptionA = "Titanic", OptionB = "Avengers: Endgame", OptionC = "Avatar", OptionD = "Star Wars: The Force Awakens", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "In what city is the film La La Land primarily set?", Category = "General Movie Trivia", OptionA = "New York", OptionB = "Chicago", OptionC = "Los Angeles", OptionD = "San Francisco", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film franchise features the character James Bond?", Category = "General Movie Trivia", OptionA = "Mission Impossible", OptionB = "The Bourne Identity", OptionC = "Spy", OptionD = "007", CorrectOption = 'D' },
            new TriviaQuestion { QuestionText = "What does CGI stand for in filmmaking?", Category = "General Movie Trivia", OptionA = "Computer Generated Images", OptionB = "Computer Graphic Imagery", OptionC = "Computer Generated Imagery", OptionD = "Creative Graphic Integration", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film was the first to use CGI for a main character?", Category = "General Movie Trivia", OptionA = "The Abyss", OptionB = "Terminator 2", OptionC = "Jurassic Park", OptionD = "The Matrix", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "What is the name of the fictional African country in Black Panther?", Category = "General Movie Trivia", OptionA = "Zamunda", OptionB = "Genosha", OptionC = "Wakanda", OptionD = "Narobia", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "How long is the runtime of Avengers: Endgame?", Category = "General Movie Trivia", OptionA = "2h 40min", OptionB = "3h 2min", OptionC = "2h 52min", OptionD = "3h 15min", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which film features a shark terrorizing a beach town?", Category = "General Movie Trivia", OptionA = "The Deep", OptionB = "Jaws", OptionC = "Open Water", OptionD = "Deep Blue Sea", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "In which decade was the Golden Age of Hollywood?", Category = "General Movie Trivia", OptionA = "1920s", OptionB = "1930s and 1940s", OptionC = "1950s", OptionD = "1960s", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "What is the name of the technology used to make actors look younger in films?", Category = "General Movie Trivia", OptionA = "Face replacement", OptionB = "De-aging", OptionC = "Digital rejuvenation", OptionD = "Youth rendering", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film popularized the term \"blockbuster\"?", Category = "General Movie Trivia", OptionA = "Star Wars", OptionB = "Jaws", OptionC = "The Godfather", OptionD = "Superman", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "What was the first feature-length animated film?", Category = "General Movie Trivia", OptionA = "Bambi", OptionB = "Pinocchio", OptionC = "Snow White and the Seven Dwarfs", OptionD = "Fantasia", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "Which streaming platform produced Roma (2018)?", Category = "General Movie Trivia", OptionA = "Amazon Prime", OptionB = "Hulu", OptionC = "Netflix", OptionD = "Apple TV+", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "What film genre features singing and dancing as part of the story?", Category = "General Movie Trivia", OptionA = "Opera", OptionB = "Musical", OptionC = "Fantasy", OptionD = "Drama", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which country has the largest film industry by number of films produced?", Category = "General Movie Trivia", OptionA = "United States", OptionB = "China", OptionC = "India", OptionD = "Japan", CorrectOption = 'C' },
            new TriviaQuestion { QuestionText = "What is the term for the list of credits at the end of a film?", Category = "General Movie Trivia", OptionA = "Epilogue", OptionB = "End titles", OptionC = "Outro", OptionD = "Roll call", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "Which film features the fictional Overlook Hotel?", Category = "General Movie Trivia", OptionA = "Psycho", OptionB = "The Shining", OptionC = "Misery", OptionD = "Doctor Sleep", CorrectOption = 'B' },
            new TriviaQuestion { QuestionText = "What does the term \"mise-en-scene\" refer to in film?", Category = "General Movie Trivia", OptionA = "The film score", OptionB = "Everything visible on screen", OptionC = "The editing style", OptionD = "The screenplay structure", CorrectOption = 'B' },
        };

        this.context.TriviaQuestions.AddRange(trivia);
        await this.context.SaveChangesAsync(ct);
    }
}

