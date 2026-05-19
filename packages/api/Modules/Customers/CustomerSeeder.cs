using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Customers;

public static class CustomerSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Customers.AnyAsync())
            return;

        dbContext.Customers.AddRange(
            new Customer { Id = Guid.NewGuid(), Number = 1,  Name = "Alpha",        Street = "Keizerslaan 1",       Zip = "1000", City = "Brussel",        Country = "Belgium", ContactName = "Alice Alpha",        ContactEmail = "alice@alpha.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 2,  Name = "Bravo",        Street = "Meir 2",              Zip = "2000", City = "Antwerpen",      Country = "Belgium", ContactName = "Bob Bravo",          ContactEmail = "bob@bravo.be"          },
            new Customer { Id = Guid.NewGuid(), Number = 3,  Name = "Charlie",      Street = "Veldstraat 3",        Zip = "9000", City = "Gent",           Country = "Belgium", ContactName = "Carol Charlie",      ContactEmail = "carol@charlie.be"      },
            new Customer { Id = Guid.NewGuid(), Number = 4,  Name = "Delta",        Street = "Steenweg 4",          Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Dave Delta",         ContactEmail = "dave@delta.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 5,  Name = "Echo",         Street = "Lippenslaan 5",       Zip = "8300", City = "Knokke",         Country = "Belgium", ContactName = "Eve Echo",           ContactEmail = "eve@echo.be"           },
            new Customer { Id = Guid.NewGuid(), Number = 6,  Name = "Foxtrot",      Street = "Stationsstraat 6",    Zip = "8500", City = "Kortrijk",       Country = "Belgium", ContactName = "Frank Foxtrot",      ContactEmail = "frank@foxtrot.be"      },
            new Customer { Id = Guid.NewGuid(), Number = 7,  Name = "Golf",         Street = "Grote Markt 7",       Zip = "7000", City = "Mons",           Country = "Belgium", ContactName = "Grace Golf",         ContactEmail = "grace@golf.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 8,  Name = "Hotel",        Street = "Rue de la Loi 8",     Zip = "1040", City = "Etterbeek",      Country = "Belgium", ContactName = "Henry Hotel",        ContactEmail = "henry@hotel.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 9,  Name = "India",        Street = "Bondgenotenlaan 9",   Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Irene India",        ContactEmail = "irene@india.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 10, Name = "Juliet",       Street = "Brusselsestraat 10",  Zip = "2800", City = "Mechelen",       Country = "Belgium", ContactName = "Jack Juliet",        ContactEmail = "jack@juliet.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 11, Name = "Kilo",         Street = "Naamsestraat 11",     Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Karen Kilo",         ContactEmail = "karen@kilo.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 12, Name = "Lima",         Street = "Leopoldlaan 12",      Zip = "3500", City = "Hasselt",        Country = "Belgium", ContactName = "Leo Lima",           ContactEmail = "leo@lima.be"           },
            new Customer { Id = Guid.NewGuid(), Number = 13, Name = "Mike",         Street = "Rue Neuve 13",        Zip = "1000", City = "Brussel",        Country = "Belgium", ContactName = "Maria Mike",         ContactEmail = "maria@mike.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 14, Name = "November",     Street = "Koningin Astridlaan 14", Zip = "2550", City = "Kontich",    Country = "Belgium", ContactName = "Nick November",      ContactEmail = "nick@november.be"      },
            new Customer { Id = Guid.NewGuid(), Number = 15, Name = "Oscar",        Street = "Nieuwpoortsesteenweg 15", Zip = "8400", City = "Oostende",  Country = "Belgium", ContactName = "Olivia Oscar",       ContactEmail = "olivia@oscar.be"       },
            new Customer { Id = Guid.NewGuid(), Number = 16, Name = "Papa",         Street = "Avenue Louise 16",    Zip = "1050", City = "Elsene",         Country = "Belgium", ContactName = "Peter Papa",         ContactEmail = "peter@papa.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 17, Name = "Quebec",       Street = "Diestsestraat 17",    Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Quinn Quebec",       ContactEmail = "quinn@quebec.be"       },
            new Customer { Id = Guid.NewGuid(), Number = 18, Name = "Romeo",        Street = "Graanmarkt 18",       Zip = "9300", City = "Aalst",          Country = "Belgium", ContactName = "Rachel Romeo",       ContactEmail = "rachel@romeo.be"       },
            new Customer { Id = Guid.NewGuid(), Number = 19, Name = "Sierra",       Street = "Rue du Midi 19",      Zip = "1000", City = "Brussel",        Country = "Belgium", ContactName = "Sam Sierra",         ContactEmail = "sam@sierra.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 20, Name = "Tango",        Street = "Schuttershofstraat 20", Zip = "2000", City = "Antwerpen",   Country = "Belgium", ContactName = "Tina Tango",         ContactEmail = "tina@tango.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 21, Name = "Uniform",      Street = "Vlamingenstraat 21",  Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Ulric Uniform",      ContactEmail = "ulric@uniform.be"      },
            new Customer { Id = Guid.NewGuid(), Number = 22, Name = "Victor",       Street = "Kloosterstraat 22",   Zip = "2000", City = "Antwerpen",      Country = "Belgium", ContactName = "Vera Victor",        ContactEmail = "vera@victor.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 23, Name = "Whiskey",      Street = "Martelarenplein 23",  Zip = "3000", City = "Leuven",         Country = "Belgium", ContactName = "Walter Whiskey",     ContactEmail = "walter@whiskey.be"     },
            new Customer { Id = Guid.NewGuid(), Number = 24, Name = "X-Ray",        Street = "Frankrijklei 24",     Zip = "2000", City = "Antwerpen",      Country = "Belgium", ContactName = "Xena Xray",          ContactEmail = "xena@xray.be"          },
            new Customer { Id = Guid.NewGuid(), Number = 25, Name = "Yankee",       Street = "Rue Royale 25",       Zip = "1000", City = "Brussel",        Country = "Belgium", ContactName = "Yves Yankee",        ContactEmail = "yves@yankee.be"        },
            new Customer { Id = Guid.NewGuid(), Number = 26, Name = "Zulu",         Street = "Wijnegem 26",         Zip = "2110", City = "Wijnegem",       Country = "Belgium", ContactName = "Zoe Zulu",           ContactEmail = "zoe@zulu.be"           },
            new Customer { Id = Guid.NewGuid(), Number = 27, Name = "Amber",        Street = "Turnhoutsebaan 27",   Zip = "2140", City = "Borgerhout",     Country = "Belgium", ContactName = "Adam Amber",         ContactEmail = "adam@amber.be"         },
            new Customer { Id = Guid.NewGuid(), Number = 28, Name = "Blaze",        Street = "Rue de Namur 28",     Zip = "5000", City = "Namen",          Country = "Belgium", ContactName = "Bianca Blaze",       ContactEmail = "bianca@blaze.be"       },
            new Customer { Id = Guid.NewGuid(), Number = 29, Name = "Cobalt",       Street = "Hasseltweg 29",       Zip = "3600", City = "Genk",           Country = "Belgium", ContactName = "Carlos Cobalt",      ContactEmail = "carlos@cobalt.be"      },
            new Customer { Id = Guid.NewGuid(), Number = 30, Name = "Dusk",         Street = "Rijselsestraat 30",   Zip = "8900", City = "Ieper",          Country = "Belgium", ContactName = "Diana Dusk",         ContactEmail = "diana@dusk.be"         }
        );
        await dbContext.SaveChangesAsync();
    }
}
