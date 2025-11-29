using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using transitions = System.Collections.Generic.List<(string StareDePlecare, char CaracterDeTranzitieh, string StareInCareAjunge)>;
using stari = (string StareInitiala, System.Collections.Generic.List<string> StariFinale, System.Collections.Generic.List<string> ToateStarile);
using System.Runtime.Serialization;
using System.Net.Sockets;

class Program
{

    public class AFN
    {
        public stari States { get; set; }
        public transitions Transitions { get; set; }

        public List<char> vocabulary { get; set; }


        public AFN() { }

        public AFN(stari states, transitions transitions, List<char> vocabulary)
        {
            States = states;
            Transitions = transitions;
            this.vocabulary = vocabulary;
        }

        public AFN(Dictionary<stari, transitions> afnDict, List<char> vocabulary)
        {
            var state = afnDict.Keys.First();
            States = state;
            Transitions = afnDict.Values.First();
            this.vocabulary = vocabulary;
        }

        public void PrintAFN()
        {
            
            Console.WriteLine("\n AFN: \n");

            Console.WriteLine(
                "AFN States:\n" +
                $"\tInitial State: {States.StareInitiala}\n" +
                $"\tFinal States: {string.Join(", ", States.StariFinale)}\n" +
                $"\tAll States: {string.Join(", ", States.ToateStarile)}\n" +
                "Transitions:"
            );
            foreach (var transition in Transitions)
            {
                Console.WriteLine($"\tFrom {transition.StareDePlecare} --{transition.CaracterDeTranzitieh}--> {transition.StareInCareAjunge}");
            }
            
        }

    }

    public class AFD
    {
        public stari States { get; set; }
        public transitions Transitions { get; set; }

        public string vocabulary { get; set; }

        public AFD() { }
        public AFD(stari states, transitions transitions, string vocab)
        {
            States = states;
            Transitions = transitions;
            vocabulary = vocab;
        }

        public void PrintAFD()
        {
            Console.WriteLine("\n AFD: \n");
            Console.WriteLine(
                "AFD States:\n" +
                $"\tInitial State: {States.StareInitiala}\n" +
                $"\tFinal States: {string.Join(", ", States.StariFinale)}\n" +
                $"\tAll States: {string.Join(", ", States.ToateStarile)}\n" +
                "Transitions:"
            );
            foreach (var transition in Transitions)
            {
                Console.WriteLine($"\tFrom {transition.StareDePlecare} --{transition.CaracterDeTranzitieh}--> {transition.StareInCareAjunge}");
            }
        }

        public bool IsStringAccepted(string input)
        {
            string currentState = States.StareInitiala;

            if (States.StariFinale.Contains(currentState) && input == "")
                return true;

            bool transitionNotFound = true;
            foreach (char symbol in input)
            {
                transitionNotFound = true;
                foreach (var transition in Transitions)
                {
                    if (transition.StareDePlecare == currentState && transition.CaracterDeTranzitieh == symbol)
                    {
                        Console.WriteLine( currentState + " " + transition.CaracterDeTranzitieh + " " );
                        currentState = transition.StareInCareAjunge;
                        transitionNotFound = false;
                        Console.WriteLine(currentState);
                        break;
                    }
                }
                if (transitionNotFound)
                    break;
            }

            return (States.StariFinale.Contains(currentState) && !transitionNotFound);
        }

        public bool VerifyAutomaton()
        {
            // Verify initial state is in Q
            if (!States.ToateStarile.Contains(States.StareInitiala))
            {
                Console.WriteLine("Error: Initial state is not in the set of states Q");
                return false;
            }

            // Verify all final states are in Q
            foreach (var finalState in States.StariFinale)
            {
                if (!States.ToateStarile.Contains(finalState))
                {
                    Console.WriteLine($"Error: Final state '{finalState}' is not in the set of states Q");
                    return false;
                }
            }

            // Verify all transitions contain only states from Q and symbols from vocabulary
            foreach (var transition in Transitions)
            {
                // Check if the source state is in Q
                if (!States.ToateStarile.Contains(transition.StareDePlecare))
                {
                    Console.WriteLine($"Error: Source state '{transition.StareDePlecare}' in transition is not in Q");
                    return false;
                }

                // Check if the destination state is in Q
                if (!States.ToateStarile.Contains(transition.StareInCareAjunge))
                {
                    Console.WriteLine($"Error: Destination state '{transition.StareInCareAjunge}' in transition is not in Q");
                    return false;
                }

                // Check if the symbol is in the vocabulary
                if (!vocabulary.Contains(transition.CaracterDeTranzitieh.ToString()))
                {
                    Console.WriteLine($"Error: Symbol '{transition.CaracterDeTranzitieh}' in transition is not in vocabulary");
                    return false;
                }
            }

            Console.WriteLine("Automaton is valid!");
            return true;
        }

        public void PrintAutomaton()
        {
            Console.WriteLine("\n=== Transition Function Table (delta) ===\n");
            Console.WriteLine("Format: delta(state, symbol) -> next_state\n");

            // Create a table for easier visualization
            // Header
            Console.Write("State\\Symbol");
            foreach (char symbol in vocabulary.ToCharArray())
            {
                Console.Write($"\t{symbol}");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', (vocabulary.Length + 1) * 8));

            // Rows for each state
            foreach (var state in States.ToateStarile)
            {
                Console.Write(state);
                foreach (char symbol in vocabulary.ToCharArray())
                {
                    // Find transition
                    bool found = false;
                    foreach (var transition in Transitions)
                    {
                        if (transition.StareDePlecare == state && transition.CaracterDeTranzitieh == symbol)
                        {
                            Console.Write($"\t{transition.StareInCareAjunge}");
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.Write("\t-");
                    }
                }

                // Mark final states
                if (States.StariFinale.Contains(state))
                {
                    Console.Write(" *");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\n* Marks final states");
            Console.WriteLine($"\nInitial state: {States.StareInitiala}");
            Console.WriteLine($"Final states: {string.Join(", ", States.StariFinale)}");
            Console.WriteLine($"Vocabulary: {vocabulary}");
        }
    }

    //  Reading regex patterns from a file

    public static string ReadingRegexFromFile(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            //read char by char from file - to manage parenthesis correctly
            
            var patterns = new List<char>();
            bool isOpenParenthesis = false;
            using (StreamReader sr = new StreamReader(filePath))
            {
                int currentChar;
                while ((currentChar = sr.Read()) != -1)
                {
                    patterns.Add((char)currentChar);
                    if ((char)currentChar == '(')
                    {
                        isOpenParenthesis = true;
                    }
                    else if ((char)currentChar == ')')
                    {
                        isOpenParenthesis = false;
                        patterns.RemoveRange(patterns.Count - 2, 1); // remove the last added '.' before ')'
                    }
                    else if (isOpenParenthesis == true && (char.IsLetterOrDigit((char)currentChar)))
                    {
                        
                        patterns.Add('.');
                    }
                }
            }
            string[] strings = new string[patterns.Count];
            Console.WriteLine("Read regex patterns:");
            Console.WriteLine(string.Format(string.Join("", patterns)));
            return string.Format(string.Join("", patterns));
        }
        else
        {
            Console.WriteLine("File not found.");
            return string.Empty;
        }
    }

    public static string RewritingRegexPatterns(string patterns)
    {
        var rewrittenPatterns = new List<string>();
        Stack<char> bookStack = new Stack<char>();
        

        foreach (var pattern in patterns)
        {
            //daca e litera sau cifra se adauga direct in lista
            if (char.IsLetterOrDigit(pattern))
            {
                rewrittenPatterns.Add(pattern.ToString());
                
            }
            else
            {
                //first - see if the priority is correct for adding items to the stack
                if (pattern == '(')
                {
                    bookStack.Push(pattern);
                    
                }
                else if (pattern == ')')
                {
                    while (bookStack.Count > 0 && bookStack.Peek() != '(')
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    if (bookStack.Count > 0 && bookStack.Peek() == '(')
                    {
                        bookStack.Pop();
                    }
                }
                else if (pattern == '+' || pattern == '*') // both have the same priority, the biggest one
                {
                    bookStack.Push(pattern);
                }
                else if (pattern == '.')
                {

                    while (bookStack.Count > 0 && (bookStack.Peek() == '+' || bookStack.Peek() == '*'))
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    bookStack.Push(pattern);

                }
                else if (pattern == '|')
                {

                    while (bookStack.Count > 0 && (bookStack.Peek() == '+' || bookStack.Peek() == '*' || bookStack.Peek() == '.'))
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    bookStack.Push(pattern);

                }
            }
        }
        while (bookStack.Count > 0)
        {
            rewrittenPatterns.Add(bookStack.Pop().ToString());
        }
        return string.Format(string.Join("", rewrittenPatterns));
    }


    // Creating AFN from regex
    public static (Dictionary<stari, transitions>,List<char>) CreateAFNFromRegex(string regex)
    {
        Stack<Dictionary<stari, transitions>> afnStack = new Stack<Dictionary<stari, transitions>>(); //explciatie: vector cu toate starile din adf, si lista de tranzitii

        int numberOfStates = 0;

        List<char> vocabulary = new List<char>();

        foreach (var c in regex)
        {
            if (char.IsLetterOrDigit(c))
            {
                //adaugam la vocabular caracterul c
                if (!vocabulary.Contains(c))
                {
                    vocabulary.Add(c);
                }
                //cream un afn pentru caracterul c (deci din 2 stari si o tranzitie intre ele, o staer e initala si o stare e finala)
                Dictionary<stari, transitions> afnForChar = new Dictionary<stari, transitions>();
                stari statesForAFN = new stari();
                statesForAFN = ($"q{numberOfStates}", new List<string> { $"q{numberOfStates + 1}" }, new List<string> { $"q{numberOfStates}", $"q{numberOfStates + 1}" });
                transitions transitionsForAFN = new transitions();
                transitionsForAFN.Add((($"q{numberOfStates}"), c, ($"q{numberOfStates + 1}")));
                afnForChar.Add(statesForAFN, transitionsForAFN);
                afnStack.Push(afnForChar);
                numberOfStates += 2;
            }
            else
            {
                if (c == '.')
                {
                    concatenareAFN(afnStack);

                }
                else if (c == '|')
                {
                    ORopperator(afnStack, numberOfStates);
                    numberOfStates += 2;
                }
                else if (c == '*')
                {
                    STARopperator(afnStack, numberOfStates);
                }
                else if (c == '+')
                {
                    PLUSopperator(afnStack, numberOfStates);
                }
            }
        }
        
        return (afnStack.Pop(),vocabulary);
    }


    public static void concatenareAFN( Stack<Dictionary<stari, transitions>> afnStack)
    {
        //concatenare, deci unim ultimele 2 afn-uri din stiva
        Dictionary<stari, transitions> afn2 = afnStack.Pop();
        Dictionary<stari, transitions> afn1 = afnStack.Pop();
        //UNIM STARI afn1 si afn2

        //starea din mijloc se uneste in una singura(o sa o schimbam in ex. q1=q2)
        Dictionary<stari, transitions> concatenatedAFN = new Dictionary<stari, transitions>();

        //parcurgem starile finale din afn1 si le legam de starea initiala din afn2

        var afn1State = afn1.Keys.First();
        var afn2State = afn2.Keys.First();
        
        List<string> updatedFinalStates = new List<string>();
        List<string> mergedAllStates = new List<string>();

        //add initial state from afn1
        mergedAllStates.Add(afn1State.StareInitiala);

        // Add merged final states from afn1
        foreach (var state in afn1State.StariFinale)
        { 
            mergedAllStates.Add(state + '=' + afn2State.StareInitiala);
        }
        
        // Add final states from afn2
        foreach (var state in afn2State.StariFinale)
        {
            updatedFinalStates.Add(state); //doar astea o sa fie finale in noul afn
            mergedAllStates.Add(state);
        }
        
        // Add other states from afn1 (not final, not initial)
        foreach (var state in afn1State.ToateStarile)
        {
            if (state != afn1State.StareInitiala && !afn1State.StariFinale.Contains(state))
            {
                mergedAllStates.Add(state);
            }
        }
        
        // Add other states from afn2 (not initial, not final)
        foreach (var state in afn2State.ToateStarile)
        {
            if (state != afn2State.StareInitiala && !afn2State.StariFinale.Contains(state))
            {
                mergedAllStates.Add(state);
            }
        }

        // Create the new state tuple once with all data
        stari statesForNewAFN = (afn1State.StareInitiala, updatedFinalStates, mergedAllStates);

        //unim tranzitiile

        transitions transitionsForNewAFN = new transitions();

     
        foreach (var transition in afn1.Values.First())
        {
            bool leadsToFinalState = false;
            foreach (var finalState in afn1State.StariFinale)
            {
                if (transition.StareInCareAjunge == finalState)
                {
                    leadsToFinalState = true;
                    //adauagam tranzitiile care se duc in stari finale sa fie modificate
                    transitionsForNewAFN.Add((transition.StareDePlecare, transition.CaracterDeTranzitieh, finalState + '=' + afn2State.StareInitiala));
                }
            }
            if (!leadsToFinalState)
            {
                //adaugam tranzitiile din afn1 care nu duc la starile finale
                transitionsForNewAFN.Add(transition);
            }
        }
 
        //adaugam tranzitiile din afn2 care nu pleaca din stare initiala
        foreach (var transition in afn2.Values.First())
        {
            if (transition.StareDePlecare != afn2State.StareInitiala)
            {
                transitionsForNewAFN.Add(transition);
            }
        }

        //adaugam tranzitiile care leaga starile finale din afn1 cu starea initiala din afn2
        foreach (var finalState in afn1State.StariFinale)
        {
            foreach (var transition in afn2.Values.First())
            {
                if (transition.StareDePlecare == afn2State.StareInitiala)
                {
                    transitionsForNewAFN.Add((finalState + '=' + afn2State.StareInitiala, transition.CaracterDeTranzitieh, transition.StareInCareAjunge));
                }
            }
        }

        concatenatedAFN.Add(statesForNewAFN, transitionsForNewAFN);
        afnStack.Push(concatenatedAFN);


    }


    public static void ORopperator(Stack<Dictionary<stari, transitions>> afnStack, int numberOfStates)
    {
        //OR operator, deci cream o noua stare initiala si o noua stare finala
        Dictionary<stari, transitions> afn2 = afnStack.Pop();
        Dictionary<stari, transitions> afn1 = afnStack.Pop();

        Dictionary<stari, transitions> orAFN = new Dictionary<stari, transitions>();

        List<string> allStates = new List<string>();

        //only one final state, a new one
        List<string> finalStates = new List<string>();
        finalStates.Add($"q{numberOfStates}");

        //only one initial state, a new one
        string newInitialState = $"q{numberOfStates + 1}";

        //adding all the states from afn1 and afn2 just how they are + he new initial and final states
        var afn1State = afn1.Keys.First();
        var afn2State = afn2.Keys.First();

        allStates.Add(newInitialState);

        foreach (var state in afn1State.ToateStarile)
        {
            allStates.Add(state);
        }
        foreach (var state in afn2State.ToateStarile)
        {
            allStates.Add(state);
        }

        allStates.Add(finalStates[0]);

        //adding the transitions from afn1 and afn2 just how they are for now
        transitions transitionsForNewAFN = new transitions();
        foreach (var transition in afn1.Values.First())
        {
            transitionsForNewAFN.Add(transition);
        }
        foreach (var transition in afn2.Values.First())
        {
            transitionsForNewAFN.Add(transition);
        }
        //adding the lambda transitions from the new initial state to the initial states of afn1 and afn2
        transitionsForNewAFN.Add((newInitialState, '~', afn1State.StareInitiala)); //we will assume ~ will never be part of the vocab
        transitionsForNewAFN.Add((newInitialState, '~', afn2State.StareInitiala));
        //adding the lambda transitions from the final states of afn1 and afn2 to the new final state
        foreach (var finalState in afn1State.StariFinale)
        {
            transitionsForNewAFN.Add((finalState, '~', finalStates[0]));
        }
        foreach (var finalState in afn2State.StariFinale)
        {
            transitionsForNewAFN.Add((finalState, '~', finalStates[0]));
        }
        stari statesForNewAFN = (newInitialState, finalStates, allStates);
        orAFN.Add(statesForNewAFN, transitionsForNewAFN);
        afnStack.Push(orAFN);

    }


    public static void STARopperator(Stack<Dictionary<stari, transitions>> afnStack, int numberOfStates)
    {
        //STAR operator, deci cream o noua stare initiala si o noua stare finala
        Dictionary<stari, transitions> afn = afnStack.Pop(); // * este operat unar
        Dictionary<stari, transitions> starAFN = new Dictionary<stari, transitions>();

        List<string> allStates = new List<string>();
        //only one initial state, a new one
        string newInitialState = $"q{numberOfStates}";

        //two initial states, a new one and the new initial state (we want to allow lambda to be accepted)
        List<string> finalStates = new List<string>();
        finalStates.Add(newInitialState); //the new initial state is also final
        finalStates.Add($"q{numberOfStates + 1}");
        

        //adding all the states from afn just how they are + he new initial and final states
        var afnState = afn.Keys.First();
        allStates.Add(newInitialState);
        foreach (var state in afnState.ToateStarile)
        {
            allStates.Add(state);
        }
        allStates.Add(finalStates[0]);

        //adding the transitions from afn just how they are for now
        transitions transitionsForNewAFN = new transitions();
        foreach (var transition in afn.Values.First())
        {
            transitionsForNewAFN.Add(transition);
        }

        //adding the lambda transitions from the new new initial state

        //we have 2 lambda transitions from the new initial state: one to the old initial state and one to the new final state
        transitionsForNewAFN.Add((newInitialState, '~', afnState.StareInitiala)); //we will assume ~ will never be part of the vocab
        transitionsForNewAFN.Add((newInitialState, '~', finalStates[0]));
        //adding the lambda transitions from the final states of afn to the new final state and to the old initial state(to allow repetition)
        foreach (var finalState in afnState.StariFinale)
        {
            transitionsForNewAFN.Add((finalState, '~', finalStates[0]));
            transitionsForNewAFN.Add((finalState, '~', afnState.StareInitiala));
        }

        stari statesForNewAFN = (newInitialState, finalStates, allStates);
        starAFN.Add(statesForNewAFN, transitionsForNewAFN);
        afnStack.Push(starAFN);
    }

    public static void PLUSopperator(Stack<Dictionary<stari, transitions>> afnStack, int numberOfStates)
    {
        //PLUS operator, similar to STAR but the new initial state is not final
        Dictionary<stari, transitions> afn = afnStack.Pop(); // + este operat unar
        Dictionary<stari, transitions> plusAFN = new Dictionary<stari, transitions>();

        List<string> allStates = new List<string>();
        //only one initial state, a new one
        string newInitialState = $"q{numberOfStates}";

        //one final state, the new one, we don't want to allow lambda to be accepted
        List<string> finalStates = new List<string>();
        finalStates.Add($"q{numberOfStates + 1}");


        //adding all the states from afn just how they are + the new initial and final states
        var afnState = afn.Keys.First();
        allStates.Add(newInitialState);
        foreach (var state in afnState.ToateStarile)
        {
            allStates.Add(state);
        }
        allStates.Add(finalStates[0]);

        //adding the transitions from afn just how they are for now
        transitions transitionsForNewAFN = new transitions();
        foreach (var transition in afn.Values.First())
        {
            transitionsForNewAFN.Add(transition);
        }

        //adding the lambda transitions from the new new initial state

        //we have 1 lambda transitions from the new initial state: one to the old initial state 
        transitionsForNewAFN.Add((newInitialState, '~', afnState.StareInitiala)); //we will assume ~ will never be part of the vocab
        
        //adding the lambda transitions from the final states of afn to the new final state and to the old initial state(to allow repetition)
        foreach (var finalState in afnState.StariFinale)
        {
            transitionsForNewAFN.Add((finalState, '~', finalStates[0]));
            transitionsForNewAFN.Add((finalState, '~', afnState.StareInitiala));
        }

        stari statesForNewAFN = (newInitialState, finalStates, allStates);
        plusAFN.Add(statesForNewAFN, transitionsForNewAFN);
        afnStack.Push(plusAFN);

    }


    // Creating AFD from AFN

    public static AFD ConvertAFNtoAFD(AFN afn)
    {
        // "lambda inchid" starea initiala
        string initialState = afn.States.StareInitiala;
        
        List<string> initialClosure = LambdaClosure(afn, initialState);

        // noua stare initiala a afd-ului
        int newNumberOfStates = 0;
        initialState = "Q" + newNumberOfStates.ToString();
        newNumberOfStates++;

        // coada pentru starile de procesat
        Queue<List<string>> statesToProcess = new Queue<List<string>>();
        statesToProcess.Enqueue(initialClosure);

        // dictionar pentru starile afd-ului
        Dictionary<string, List<string>> afdStates = new Dictionary<string, List<string>>();
        afdStates.Add(initialState, initialClosure);

        // HashSet to track processed states to avoid infinite loops
        HashSet<string> processedStates = new HashSet<string>();

        // lista pentru tranzitiile afd-ului
        transitions afdTransitions = new transitions();

        while(statesToProcess.Count > 0)
        {
            List<string> currentState = statesToProcess.Dequeue();
            string currentStateName = afdStates.FirstOrDefault(x => x.Value.SequenceEqual(currentState)).Key;
            
            // Skip if already processed
            if (processedStates.Contains(currentStateName))
                continue;
            processedStates.Add(currentStateName);

            foreach (char symbol in afn.vocabulary)
            {
                //adaugam fiecare tranzitie posibila cu un anumit caracter din vocabular fara lambda inchidere
                List<string> newStateWithoutLambda = new List<string>();
                foreach (string state in currentState)
                {
                    foreach (var transition in afn.Transitions)
                    {
                        if (transition.StareDePlecare == state && transition.CaracterDeTranzitieh == symbol)
                        {
                            newStateWithoutLambda.Add(transition.StareInCareAjunge);
                        }
                    }
                }
                //adaugam unde ne duce lambda inchiderea pentru fiecare stare gasita mai sus
                List<string> newStateList = new List<string>();
                foreach (string state in newStateWithoutLambda)
                {
                    List<string> lambdaClosure = LambdaClosure(afn, state);
                    foreach (string closureState in lambdaClosure)
                    {
                        if (!newStateList.Contains(closureState))
                        {
                            newStateList.Add(closureState);
                        }
                    }
                }
                if (newStateList.Count > 0)
                {
                    string newStateName;
                    // Check if this state already exists using SequenceEqual
                    var existingState = afdStates.FirstOrDefault(x => x.Value.SequenceEqual(newStateList));
                    
                    if (existingState.Key == null)
                    {
                        newStateName = "Q" + newNumberOfStates.ToString();
                        newNumberOfStates++;
                        afdStates.Add(newStateName, newStateList);
                        statesToProcess.Enqueue(newStateList);
                    }
                    else
                    {
                        newStateName = existingState.Key;
                    }
                    afdTransitions.Add((currentStateName, symbol, newStateName));
                }
            }
        }

        // determinam starile finale ale afd-ului
        List<string> afdFinalStates = new List<string>();
        foreach (var afdState in afdStates)
        {
            foreach (string afnFinalState in afn.States.StariFinale)
            {
                if (afdState.Value.Contains(afnFinalState))
                {
                    afdFinalStates.Add(afdState.Key);
                    break;
                }
            }
        }
        // cream starea afd-ului
        stari afdStari = (initialState, afdFinalStates, afdStates.Keys.ToList());
        AFD afd = new AFD(afdStari, afdTransitions, string.Join("", afn.vocabulary));
        return afd;
    }

    public static List<string> LambdaClosure(AFN afn, string initialState)
    {
        List<string> closure = new List<string>();
        Stack<string> stack = new Stack<string>();
        stack.Push(initialState);
        while (stack.Count > 0)
        {
            string state = stack.Pop();
            if (!closure.Contains(state))
            {
                closure.Add(state);
                foreach (var transition in afn.Transitions)
                {
                    if (transition.StareDePlecare == state && transition.CaracterDeTranzitieh == '~')
                    {
                        stack.Push(transition.StareInCareAjunge);
                    }
                }
            }
        }
        return closure;

    }

    // RegexToDFA function - converts a regex pattern to a DFA
    public static AFD RegexToDFA(string regexPattern)
    {
        // Read and preprocess the regex pattern
        string preprocessedRegex = regexPattern;
        
        // Rewrite the regex to postfix notation
        string postfixRegex = RewritingRegexPatterns(preprocessedRegex);
        
        // Create AFN from the postfix regex
        (Dictionary<stari, transitions> afnDict, List<char> vocabulary) = CreateAFNFromRegex(postfixRegex);
        AFN afn = new AFN(afnDict, vocabulary);
        
        // Convert AFN to DFA
        AFD afd = ConvertAFNtoAFD(afn);
        
        return afd;
    }

    static void Main(string[] args)
    {
       string initialString = ReadingRegexFromFile("filepath");
       string rewrittenString = RewritingRegexPatterns(initialString);
         Console.WriteLine("Rewritten regex patterns:");
         Console.WriteLine(rewrittenString);
         
       /* (Dictionary<stari, transitions> afnDict, List<char> vocabulary) = CreateAFNFromRegex(rewrittenString);

        AFN afn = new AFN(afnDict,vocabulary);
        afn.PrintAFN();

        Console.WriteLine(
            $"\n Converting AFN to AFD: \n"
        );

        AFD afd = ConvertAFNtoAFD(afn);
        afd.PrintAFD();

        // Verify the automaton
        Console.WriteLine("\n=== Verifying Automaton ===");
        afd.VerifyAutomaton();

        // Print the transition table
        Console.WriteLine();
        afd.PrintAutomaton();

        
        // Testing if strings are accepted by AFD
        Console.WriteLine("\n=== Testing Strings ===\n");

        if (afd.IsStringAccepted(""))
            Console.WriteLine("Accepted");
        else Console.WriteLine("not accepted");

        if (afd.IsStringAccepted("abababababab"))
            Console.WriteLine("Accepted");
        else Console.WriteLine("not accepted");

        //this shouldn't be accepted

        if(afd.IsStringAccepted("abbaab"))
            Console.WriteLine("Accepted");
        else Console.WriteLine("not accepted");

        if(afd.IsStringAccepted("cdcd"))
            Console.WriteLine("Accepted");
        else Console.WriteLine("not accepted");

        */
        AFD afd = RegexToDFA(initialString);
        afd.VerifyAutomaton();  
        afd.PrintAutomaton();

        
    }
}
