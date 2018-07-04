﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Xml;

public class PreTraitement
{
    private List<string> projections;
    private List<string> fromClauses;
    private string id;
    private string request;
    private int i = 0;
    readonly string path_file = @"C:\Users\Yann\Desktop\test.xml";

    public PreTraitement()
    {
        this.projections = new List<string>();
        this.fromClauses = new List<string>();
    }

    public void Process()
    {
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.Load(path_file);
        XmlNodeList donnees = xmldoc.GetElementsByTagName("requête");
        try
        {
            foreach (XmlNode data in donnees)
            {
                XmlNode tmp = data.SelectSingleNode("fromClause");
                if (tmp != null)
                    if (tmp.SelectNodes("from") != null)
                        foreach (XmlNode from in tmp.SelectNodes("from"))
                            this.fromClauses.Add(from.InnerText);
                tmp = data.SelectSingleNode("projections");
                if (tmp != null)
                    if (tmp.SelectNodes("from") != null)
                        foreach (XmlNode projection in tmp.SelectNodes("projection"))
                            this.projections.Add(projection.InnerText);


                id = data.SelectSingleNode("id").InnerText;
                request = data.SelectSingleNode("request").InnerText;
                Modify();
                projections.Clear();
                fromClauses.Clear();

            }
            Console.ReadLine();
        }
        catch (Exception e) { Console.WriteLine(e.Message); Console.ReadLine(); }
    }

    public void Modify()
    {
        string tmp;
        if (projections.Count == 0)
            Console.WriteLine(++i);
        else  if (fromClauses.Count == 0)
            Console.WriteLine(++i);

        else if (fromClauses.Count == 1)
        {
            Console.WriteLine(++i);
            foreach (string projection in this.projections)
            {

                if (!projection.Equals(""))
                {
                    tmp = Improve(projection);

                    //Console.WriteLine(fromClauses[0] + "." + tmp + Environment.NewLine);

                }
            }

        }
        else
        {
            Console.WriteLine(request);
            string identifier;

            foreach (var projection in projections)
            {
                identifier = HasIdentifier(projection);
                if (identifier != "")
                {

                    if (!identifier.Equals(""))
                    {
                        // Console.WriteLine(projection);
                        string tempo = ReplaceIdentifier(request, identifier, projection);
                        Console.WriteLine();
                      //  Console.ReadLine();
                    }
                }
                else 
                    Console.WriteLine("Heloise je t'aimais");
                // Console.ReadLine();
            }
        }

    }

    public string Improve(string test)
    {
        /*si on trouve une projection du type : max(x) as y, on supprime le as y*/

        /*Permet de trouver le as y*/
        Regex regex = new Regex(@" *as|AS [A-z0-9.,-]+");
        /*supprime le as s'il existe*/

        if (regex.Match(test).Success)
            test = regex.Replace(test, "");

        /*Si projection du type : max(x)/y => ( max(x)/y ) 
         Pour cela on regarde la position de la dernière parenthèse, si elle est avant la position du '/'
         Alors on entoure la projection de ( )*/

        if (test.LastIndexOf(")") < test.LastIndexOf("/") || (test.ToLower().Contains("case") && test.ToLower().Contains("when")))
            test = "(" + test + ")";

        /*Si il y a un identifier on le supprime ex : t1.species => species*/
        if (HasIdentifier(test) != "")
            test = test.Substring(test.IndexOf('.') + 1, test.Length - test.IndexOf('.') - 1);

        /*On retourne la projection traitée*/
        return test;
    }
    public string HasIdentifier(string test)
    {
        Regex regex = new Regex(@"^\w+\.\w+|^\w+\.\*|\+ \w+\.\w+");
        Match match = regex.Match(test);
        string tempo;
        string results = "";
        while (match.Success)
        {
            tempo = match.Value.Substring(match.Value.IndexOf('.') + 1, match.Value.Length - match.Value.IndexOf('.') - 1);
            if (tempo.Length > 1 || tempo.Equals('*'))
                if (!results.Contains(tempo.ToLower()))
                    results += "|" + test.ToLower().Substring(0, test.IndexOf('.')).Replace("+", "");
            test = test.Substring(0, match.Index) + " " + test.Substring(match.Index + match.Length, test.Length - match.Index - match.Length);
            match = regex.Match(test);
        }
        if (!results.Equals(""))
        {
            results = (new Regex(@"\s+")).Replace(results, "");
            results = results.Substring(results.IndexOf("|") + 1, results.Length - results.IndexOf("|") - 1);
        }
        return results;
    }

    public string ReplaceIdentifier(string request, string identifier, string projection)
    {
        Regex regex = new Regex(@"(from|join)[\s+]*\[[\w]+\].\[[\w+ .]*\] *\w+");
        Match match = regex.Match(request.ToLower());
        string results = "";
        string tmp = "";
        while (match.Success)
        {
            results += request.Substring(match.Index + 4, match.Length - 4) + "|";
            request = request.Substring(0, match.Index) + Environment.NewLine + request.Substring(match.Index + match.Length, request.Length - match.Index - match.Length);
            match = regex.Match(request.ToLower());
        }
        //Console.WriteLine(results);
        foreach (string identi in identifier.Split('|'))
        {
            foreach (string result in results.Split('|'))
            {

                string identmp = (new Regex(@"\] *\w+")).Match(result).Value.Replace("]", "");
                identmp = (new Regex(@"\s+")).Replace(identmp, "");
                // Console.WriteLine(identmp);
                if (identmp.ToLower().Equals(identi) && !result.Equals(""))
                {
                    if (result.Contains("]"))
                    {
                        if (identi.Length == 1)
                        {
                            identmp = identi;
                            if (tmp.Equals(""))
                                tmp = projection.ToLower();
                        }
                       else if (tmp.Equals(""))
                            tmp = projection;
                        tmp = tmp.Replace(identmp + ".", (new Regex(@"\] *[\w]+").Replace(result, "]")) + ".");
                        tmp = (new Regex(@"\s+")).Replace(tmp, "");
                        Console.WriteLine(++i);
                        //  Console.WriteLine(projection + "Heloise");
                    }
                    //  
                }
            }
        }
        Console.WriteLine(tmp);
        return tmp;
    }

}


