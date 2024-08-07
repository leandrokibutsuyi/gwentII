using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
class Lexer 
{

    public List<Token> tokens = new List<Token>();

    private string a_analizar;
    
        public Lexer(string entrada)
        {
            this.a_analizar = entrada;
        }

    public List<Token> Tokenizar()
    {
        // con esto estaremos eliminando los espacios en blanco antes y despues del primer y ultimo caracter respectivamene
        a_analizar = a_analizar.Trim();
        // con esto estaremos separando cada fragmento del codigo atendiendo al lenguaje para su posterior uso
        string PalabrasReservadas = @"\b (if | else | while | for | break | continue | return | public | private | static | class | effect | card) \b";
        string TiposDeVariables = @" int | long | double | float | string | bool | char | var ";
        string Metodos = @" Find | Push | SendBottom | Pop | Remove | Shuffle ";
        string OperadorFor = "in";
        string Identificadores = @"^[a-zA-Z_][a-zA-Z0-9_]*";
        string Delimitadores = @"[\(\)\{\}\[\]]";
        string OperadoresAritmeticos = @"(?:[+\-*/%])";
        string OperadoresDeComparacion = @"(?:==|!=|>=|<=|>|<)";
        string OperadoresDeAsignacion =  @"(?:=|\+=|-=|\*=|/=|%=|:)";
        string OperadoresLogicos = @"(?:&&|\|\||!)";
        string OperadoresDeIncDec = @"(?:\+\+|--)";
        string OperadoresDeCadenas = @" @@ | @ ";
        string OperadorLanda = @"=>"; 
        string Identificadorestring = @"""(([^""\\]|\.)*?)""";
        string PatronDeNumero = @"\d+";
        string PuntoComa = @";";
        string Punto = @"\.";
        string Coma = @",";

    var Diccionario = new Dictionary<string, string>
    {
        { PalabrasReservadas, "PalabrasReservadas" },
        { TiposDeVariables, "TiposDeVariables" },
        { Metodos, "Metodos" },
        { OperadorFor , "OperadorFor" },
        { Identificadores, "Identificadores" },
        { Delimitadores, "Delimitadores" },
        { OperadoresAritmeticos, "OperadoresAritmeticos" },
        { OperadoresDeComparacion, "OperadoresDeComparacion" },
        { OperadoresDeAsignacion, "OperadoresDeAsignacion" },
        { OperadoresLogicos, "OperadoresLogicos" },
        { OperadoresDeIncDec, "OperadoresDeIncDec" },
        { OperadoresDeCadenas, "OperadoresDeCadenas" },
        { Identificadorestring, "Identificadorestring" },
        { OperadorLanda , "OperadorLanda" },
        { PatronDeNumero, "PatronDeNumero" },
        { PuntoComa , "PuntoComa" },
        { Punto , "Punto" },
        { Coma , "Coma" }

    };

    //recorro todos los tipos devtokens definidos en Diccionario. Para cada iteracion se busca una coincidencia al principio de la entrada 
    //Si encuentra una coincidencia que es más larga que cualquier coincidencia anterior actualizo<<>>juju 
    
    while(!string.IsNullOrEmpty(a_analizar))
    {
        string MejorToken = null!;
        string MejorTipoDeToken = null!;
        int IMejorCoincidencia = 0;

        foreach (var item in Diccionario)
        {
            var token = Regex.Match(a_analizar , item.Key);
            if (token.Success && token.Index == 0 && token.Length > IMejorCoincidencia)
            {
            MejorToken = token.Value;
            MejorTipoDeToken = item.Value;
            IMejorCoincidencia = token.Length;
            }
        }

        if(MejorToken == null)
        {
            a_analizar = a_analizar.Substring(1).Trim();
            continue;
        }

        tokens.Add(new Token(MejorTipoDeToken , MejorToken));
        
        a_analizar = a_analizar.Substring(IMejorCoincidencia).Trim();

        if (MejorTipoDeToken == "OperadorAritmetico" && !string.IsNullOrEmpty(a_analizar))
        {

        var SiguienteToken = Regex.Match(a_analizar, OperadoresAritmeticos);

        if (SiguienteToken.Success && SiguienteToken.Index == 0)
        {
            tokens[tokens.Count - 1].PropioToken += SiguienteToken.Value;
            a_analizar = a_analizar.Substring(SiguienteToken.Length).Trim();
        }

        } 
        }
        return tokens;
    }
}
public class Token
{
    public string TipoDeToken {get; set;}
    public string PropioToken {get; set;}
    public Token(string TipoDeToken , string PropioToken)
    {
        this.TipoDeToken = TipoDeToken;
        this.PropioToken = PropioToken;
    }
}
class Program
{
    static void Main(string[] args)
    {
        // The input string to be analyzed
        string inputString = @"
        effect {
            Name: ""Damage"",
            Params: {
                Amount: Number
            },
            Action: (targets, context) => {
                for target in targets {
                    i = 0;
                    while (i++ < Amount)
                        target.Power -= 1;
                };
            }
        }

        effect {
            Name: ""Draw"",
            Action: (targets, context) => {
                topCard = context.Deck.Pop();
                context.Hand.Add(topCard);
                context.Hand.Shuffle();
            }
        }

        effect {
            Name: ""ReturnToDeck"",
            Action: (targets, context) => {
                for target in targets {
                    owner = target.Owner;
                    deck = context.DeckOfPlayer(owner);
                    deck.Push(target);
                    deck.Shuffle();
                    context.Board.Remove(target);
                };
            }
        }

        card {
            Type: ""Oro"",
            Name: ""Beluga"",
            Faction: ""Northern Realms"",
            Power: 10,
            Range: [""Melee"", ""Ranged""],
            OnActivation: [
                {
                    Effect: {
                        Name: ""Damage"",
                        Amount: 5,
                    },
                    Selector: {
                        Source: ""board"",
                        Single: false,
                        Predicate: (unit) => unit.Faction == ""Northern"" @@ ""Realms""
                    },
                    PostAction: {
                        Type: ""ReturnToDeck"",
                        Selector: {
                            Source: ""parent"",
                            Single: false,
                            Predicate: (unit) => unit.Power < 1
                        },
                    }
                },
                {
                    Effect: ""Draw""
                }
            ]
        }";

        // Create a Lexer object with the input string
        Lexer lexer = new Lexer(inputString);

        // Call the Tokenizar method to tokenize the input
        List<Token> tokens = lexer.Tokenizar();

        // Print each token
        foreach (var token in tokens)
        {
            Console.WriteLine($"Type: {token.TipoDeToken}, Value: {token.PropioToken}");
        }
    }
}