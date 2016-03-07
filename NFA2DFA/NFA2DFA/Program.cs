/*
 * Author: Mark Clark
 * Algorithm Description: http://www.cs.may.ie/staff/jpower/Courses/Previous/parsing/node9.html
 * Class: COSC 461 Compilers
 * Assignment: PA1 - NFAtoDFA
 * 
 * All classes and regions are minimized by default for your convenience.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace NFA2DFA
{
    public class State{
        public int name;
        public Dictionary<char, HashSet<int>> connections;

        //  Initialization of State Class
        public State(int n, List<char> sigma)
        {
            this.name = n;
            this.connections = new Dictionary<char, HashSet<int>>();
            foreach(char c in sigma) this.connections.Add(c, new HashSet<int>());
        }
    }

    public class StateSet
    {
        public char name;
        public HashSet<int> states;    //  HashSet is list that does not permit duplicates
        public bool proc;

        //  Initialize StateSet
        public StateSet()
        {
            this.name = '\0';
            this.states = new HashSet<int>();
            this.proc = false;
        }
        //  Take existing StateSet and add its values to calling object
        public void join(StateSet source)
        {
            foreach(int i in source.states) this.states.Add(i);
        }

        //  Take state object and add its name to calling object
        public void join(State source)
        {
            this.states.Add(source.name);
        }
    }

    public class DFA
    {
        public StateSet q0;                                             // Initial State
        public List<StateSet> F;                                        // Final State(s)
        public int count;                                               // Total States
        public List<char> sigma;                                        // Language
        public Dictionary<StateSet, Dictionary<char, StateSet>> qDelta; // StateName:InputSymbol:StateName

        public DFA(List<char> L, StateSet q)
        {
            sigma = new List<char>();
            F = new List<StateSet>();
            count = 0;
            foreach (char c in L)
            {
                if (c != 'E') this.sigma.Add(c);
            }
            q.name = 'A';
            q0 = q;
            qDelta = new Dictionary<StateSet,Dictionary<char,StateSet>>();
            qDelta.Add(q, new Dictionary<char,StateSet>());
            count++;
        }

        public void printDFA()
        {
            //List<[same type as index]>
            List<StateSet> keyList = new List<StateSet>(qDelta.Keys);
            //for (int i = 0; i < keyList.Count; i++)
            //{
            //    int myInt = qDelta[keyList[i]];

            //}
            Console.Out.Write("Initial State:\t{0}\n", q0.name);
            Console.Out.Write("Final States:\t{"); foreach(StateSet s in F)Console.Out.Write(s.name); Console.Out.Write("}\n");
            Console.Out.Write("Total States:\t{0}\n", count);
            Console.Out.Write("State\t" + String.Join("\t\t", sigma) + "\n");
            foreach (StateSet q in keyList)
            {
                Console.Out.Write(q.name.ToString());

                bool twoSpace = false;
                for (int c = 0; c < sigma.Count; c++)
                {
                    if (twoSpace) Console.Out.Write("\t\t{");
                    else Console.Out.Write("\t{");
                    twoSpace = true;
                    if (qDelta[q].ContainsKey(sigma[c])) Console.Out.Write(qDelta[q][sigma[c]].name.ToString());
                    Console.Out.Write("}");
                }
                Console.Out.Write("\t: [");
                bool comma = false;
                foreach (int i in q.states)
                {
                    if (comma) Console.Out.Write("," + i);
                    else Console.Out.Write(i);
                    comma = true;
                }
                Console.Out.Write("]");
                Console.Out.Write("\n");
            }
        }
    }

    class NFA
    {
        public int q0;              // Initial State
        public List<int> F;         // Final State(s)
        public int count;           // Total States
        public List<char> L;        // Language
        public List<State> qDelta;  // NFA Transitions Containter

        //  NFA() Initializes the class with input from stdin, read from Main
        public NFA(List<String> input)
        {
            //  Loop thru all lines and process by RegEx Recognition
            foreach (String item in input)
            {
                #region NFA Recognize Initial State and process
                if (Regex.IsMatch(item, "^Initial State:\\s*[\\d]*\\s*$"))
                {
                    foreach (Match match in Regex.Matches(item, "^Initial State:\\s*([\\d]*)\\s*$"))
                    {
                        q0 = Convert.ToInt32(match.Groups[1].Value);
                    }
                }
                #endregion
                #region NFA Recognize Final States and process
                else if (Regex.IsMatch(item, "^Final States:\\s*\\{[\\d,]*\\}\\s*$"))
                {
                    foreach (Match match in Regex.Matches(item, "^Final States:\\s*\\{([\\d,]*)\\}\\s*$"))
                    {
                        F = match.Groups[1].Value.Split(',').Select(int.Parse).ToList();
                    }

                }
                #endregion
                #region NFA Recognize State Total and Process
                else if (Regex.IsMatch(item, "^Total States:\\s*[\\d]*\\s*$"))
                {
                    foreach (Match match in Regex.Matches(item, "^Total States:\\s*([\\d]*)\\s*$"))
                    {
                        count = Convert.ToInt32(match.Groups[1].Value);
                    }
                    qDelta = new List<State>(count);
                }
                #endregion
                #region NFA Recognize Language/sigma Line and Process
                else if (Regex.IsMatch(item, "^State[.]*"))
                {
                    String tmpStr;
                    L = new List<char>();
                    tmpStr = item.Substring(5);
                    for (int i = 0; i < tmpStr.Length; i++)
                    {
                        if (tmpStr[i] >= 'A' && tmpStr[i] <= 'z')
                        {
                            L.Add(tmpStr[i]);
                        }
                    }
                    //L = tmpStr.Split(' ').Select(char.Parse).ToList();
                }
                #endregion
                #region NFA Recognize Transition Lines and Process
                else if (Regex.IsMatch(item, "^[\\d]*[.]*"))
                {

                    int stateID = -1;
                    int iChar = 0;
                    State node = null;

                    foreach (String part in item.Split(' '))
                    {
                        
                        // New State Found, New entry in state transition list
                        if (Regex.IsMatch(part, "^[\\d]+$"))
                        {
                            stateID = Convert.ToInt32(part);
                            node = new State(stateID, L);
                            if (stateID > count)
                            {
                                Console.Error.Write("Warning: State Number exceeded Total States. Ensure Input follows assumed notation (zero or one indexed array of states)\nMax Expected: {0} Read {1}", count, stateID);
                            }
                        }
                        else if (Regex.IsMatch(part, "^\\{[,\\d]*\\}$"))
                        {
                            if (node == null) Console.Error.Write("Error: Missed State Index.\n");
                            else if (iChar > L.Count) Console.Error.Write("Error: Exceded language elements.\n");
                            else
                            {
                                String tmp = part.Trim(new char[] { '{', '}' });
                                if (tmp != "")
                                {
                                    foreach (int i in tmp.Split(new char[] { ',' }).Select(int.Parse).ToArray())
                                    {
                                        //  Add transition item
                                        if (node != null)
                                        {
                                            node.connections[L[iChar]].Add(i);
                                        }
                                    }
                                }
                                iChar++;
                            }
                        }
                        else
                        {
                            //Console.Error.Write("Error: Input file does not follow standard format.\n");
                        }
                    }
                    qDelta.Add(node);
                }
                #endregion
                #region NFA Error Segment
                else
                {
                    Console.Error.Write("Error: Input file does not follow standard format.\n");
                }
                #endregion
            }
        }

        //  The Subset Construction Algorithm
        public void NFAtoDFA()
        {
            char tmpChar = 'A';
            DFA dfa = null;
            int prevCount;
            StateSet tmpQ;
            //  1. Create the start state of the DFA by taking the epsilon-closure of the start state of the NFA.
            foreach(State q in qDelta) if (q0 == q.name) dfa = new DFA(L, eClosure(q));


            //  2. Perform the following for the new DFA state:
            //      For each possible input symbol:
            dfa.q0.proc = true;
            foreach(char c in dfa.sigma)
            {
                tmpQ = new StateSet();
                //  i. Apply move to the newly-created state and the input symbol; this will return a set of states.
                foreach(int i in dfa.q0.states)
                {
                    foreach (State q in qDelta)
                    {
                        if(q.name == i)tmpQ.join(move(q,c));
                    }
                }
                //  ii.Apply the epsilon-closure to this set of states, possibly resulting in a new set.
                HashSet<int> tmpHash = new HashSet<int>(tmpQ.states); ;
                foreach(int i in tmpHash)
                {
                    foreach (State q in qDelta)
                    {
                        if(q.name == i) tmpQ.join(eClosure(q));
                    }
                }
                if(tmpQ.states.Count > 0 && !tmpQ.states.SetEquals(dfa.q0.states))
                {
                    tmpQ.name = ++tmpChar;
                    dfa.qDelta[dfa.q0].Add(c,tmpQ);
                    dfa.qDelta.Add(tmpQ, new Dictionary<char, StateSet>());
                    dfa.count++;
                }
               //  This set of NFA states will be a single state in the DFA.
            }
            //  3. Each time we generate a new DFA state, we must apply step 2 to it. The process is complete when
            //      applying step 2 does not yield any new states.
            do
            {
                prevCount = dfa.count;
                tmpChar = findMoreStates(dfa, tmpChar);
            } while (prevCount != dfa.count);
            //  4. The finish states of the DFA are those which contain any of the finish states of the NFA.
            noteFinals(dfa);

            //  Print DFA corresponding to given NFA
            Console.Clear();
            Console.Out.Write("\n\n--------------------NFA--------------------\n");
            printNFA();
            Console.Out.Write("\n\n--------------------DFA--------------------\n");
            dfa.printDFA();
        }

        //  Each time we generate a new DFA state, we must apply step 2 to it.
        public char findMoreStates(DFA dfa, char tmpChar)
        {
            List<StateSet> keyList = new List<StateSet>(dfa.qDelta.Keys);

//            Console.Clear();
//            dfa.printDFA();
//            Thread.Sleep(1000);

            foreach(StateSet q in keyList)
            {
                if (!q.proc)
                {
                    q.proc = true;
                    StateSet tmpQ;
                    foreach (char c in dfa.sigma)
                    {
                        tmpQ = new StateSet();
                        //  i. Apply move to the newly-created state and the input symbol; this will return a set of states.
                        foreach (int i in q.states)
                        {
                            foreach (State r in qDelta)
                            {
                                if (r.name == i) tmpQ.join(move(r, c));
                            }
                        }
                        //  ii.Apply the epsilon-closure to this set of states, possibly resulting in a new set.
                        HashSet<int> tmpHash = new HashSet<int>(tmpQ.states);
                        foreach (int i in tmpHash)
                        {
                            foreach (State r in qDelta)
                            {
                                if (r.name == i) tmpQ.join(eClosure(r));
                            }
                        }
                        if (tmpQ.states.Count > 0 && !tmpQ.states.SetEquals(q.states))
                        {
                            tmpQ.name = ++tmpChar;
                            dfa.qDelta[q].Add(c, tmpQ);
                            dfa.qDelta.Add(tmpQ, new Dictionary<char, StateSet>());
                            dfa.count++;
                        }
                    }
                    //  End after single update
                    return tmpChar;
                }
            }
            return tmpChar;
        }

        public void noteFinals(DFA dfa)
        {
            List<StateSet> keyList = new List<StateSet>(dfa.qDelta.Keys);

            foreach (StateSet q in keyList)
            {
                foreach(int i in F)
                {
                    if (q.states.Contains(i)) dfa.F.Add(q);
                }
            }
        }

        /*
         * The epsilon-closure function takes a state and returns the set of states reachable
         * from it based on (one or more) epsilon-transitions. Note that this will always
         * include the state itself. We should be able to get from a state to any state in its
         * epsilon-closure without consuming any input.
         */
        public StateSet eClosure(State q)
        {
            StateSet eClosed = new StateSet();

            eClosed.join(q);

            //  Enumerate epsilon connections
            foreach (int num in q.connections['E'])
            {
                foreach (State qTmp in qDelta)
                {
                    //  NOTE:   EFFICIENCY FLAW. Too far into development to change qDelta to hashtable
                    //          -Flaw: Enumerate qDelta until we reach our desired node
                    //  While hashset prevents duplicates, we must ensure there are not infinite loops
                    if (qTmp.name == num && !eClosed.states.Contains(qTmp.name))
                    {
                        eClosed.join(qTmp); //  Add Node to list
                        eClosed.join(eClosure(qTmp));
                    }
                }
            }
            return eClosed;
        }

        /*
         * The function move takes a state and a character, and returns the set of states
         * reachable by one transition on this character.
         */
        public StateSet move(State q, char c)
        {
            StateSet delta = new StateSet();

            //  Enumerate corresponding connections
            foreach (int num in q.connections[c])
            {
                foreach (State qTmp in qDelta)
                {
                    if (qTmp.name == num)
                    {
                        delta.join(qTmp);
                    }
                }
            }
            return delta;
        }

        //  Print to confirm NFA
        public void printNFA()
        {
            Console.Out.Write("Initial State:\t{0}\n", q0);
            Console.Out.Write("Final States:\t{"+String.Join(",",F)+"}\n");
            Console.Out.Write("Total States:\t{0}\n", count);
            Console.Out.Write("State\t" + String.Join("\t\t", L) + "\n");
            foreach (State q in qDelta)
            {
                Console.Out.Write(q.name.ToString());
                bool twoSpace = false;
                for (int c = 0; c < L.Count; c++)
                {
                    if (twoSpace) Console.Out.Write("\t\t{");
                    else Console.Out.Write("\t{");
                    twoSpace = true;
                    bool comma = false;
                    foreach (int it in q.connections[L[c]])
                    {
                        if (comma) Console.Out.Write(",");
                        Console.Out.Write(it.ToString());
                        comma = true;
                    }
                    Console.Out.Write("}");
                }
                Console.Out.Write("\n");
            }
        }

    }

    class Program
    {

        static void Main(string[] args)
        {        
            NFA nfa;

            if (!Console.IsInputRedirected)
            {
                Console.Error.Write("Usage: NFA2DFA.exe < 'inputNFA.txt'");
                return;
            }

            List<String> input = new List<String>();
            string line;

            while ((line = Console.ReadLine()) != null)
            {
//DEBUG INPUT               Console.Out.Write(line + "\n");
                input.Add(line);
            }
            nfa = new NFA(input);

            nfa.NFAtoDFA();

            //  DEBUG for Breakpoint to hold console open
            if (false) Console.Error.Write("Potato");
        }

    }

}
