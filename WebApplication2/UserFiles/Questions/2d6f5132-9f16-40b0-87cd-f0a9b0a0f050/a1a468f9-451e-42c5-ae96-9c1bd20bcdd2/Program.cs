using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        //function to remove punctuations and number for strings.
        static void  removePuncationAndNumbers(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var sb = new StringBuilder();

                foreach (char c in word)
                {
                    //create new string without puncuation and numbers
                    if (!char.IsPunctuation(c) && !char.IsNumber(c))
                        sb.Append(c);
                  
                }

                words[i] = sb.ToString();
            }
        }

        static void Main(string[] args)
        {
           //variable declaration
            string line;
            string []words=new string[2000];
            int []counts=new int [2000];
            int counter = 0;
            int location = 0;

            // Read the file line by line
            System.IO.StreamReader file = new System.IO.StreamReader("test.txt");
            while ((line = file.ReadLine()) != null)
            {
                //read one line split on spaces
                var lineWords = line.Split(' ');

               Program.removePuncationAndNumbers(lineWords);
                //check each words and increase its count
                for(int i=0;i<lineWords.Length; i++)
                {
                    bool notFound = true;
                    for (int j=0;j<counter;j++)
                    {
                        //word already exists in array
                        if (words[j] == lineWords[i])
                        {
                            notFound = false;
                            location = j;
                            break;
                        }

                    }
                    //word was not in list new word
                    if (notFound)
                    {
                        words[counter] = lineWords[i];
                        counts[counter] = 1;
                        counter++;
                    }
                    else
                    {
                        //word was already in list just increase counter.
                        counts[location]= counts[location] + 1;
                    }
                    
                }
     
            }

            Console.WriteLine("Words  \t\t " + "Counts\n");
            for (int i=0;i< counter;i++)
            {
                Console.WriteLine(words[i] + " \t\t  " + counts[i]);
            }


            file.Close();

            // Suspend the screen.
            Console.ReadLine();
        }


       
    }
}
