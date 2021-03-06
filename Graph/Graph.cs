using Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtensionMethods;
using System.Windows.Input;
using System.Runtime.Serialization;
using System.IO;
using Force.DeepCloner.Helpers;
using Force.DeepCloner;


namespace Graph
{
    public interface IGraph<T>
    {
        IEnumerable<T> RoutesBetween(T source, T target);
        
    }


    public class Graph<T> : IGraph<T>
    {
        private IEnumerable<ILink<T>> links;

        public Graph(IEnumerable<ILink<T>> links)
        {
            this.links = links;
        }

        public IEnumerable<T> RoutesBetween(T source, T target)
        {


            List<Node> nodes = new List<Node>();
            List<string> points = new List<string>();
            List<List<string>> routesBetween = new List<List<string>>();

            List<T> rotas = new List<T>();
            
            string caminho;
            foreach (ILink<T> l in links)
            {
                if (!points.Contains(l.Source.ToString()))
                {
                    points.Add(l.Source.ToString());

                }
                if (!points.Contains(l.Target.ToString()))
                {
                    points.Add(l.Target.ToString());

                }


                int indexNode = nodes.FindIndex(p => p.ToString().Equals(l.Source.ToString()));
                if (indexNode < 0)
                    nodes.Add(new Node(l.Source.ToString(), l.Target.ToString()));

                indexNode = nodes.FindIndex(p => p.ToString().Equals(l.Target.ToString()));
                if (indexNode < 0)
                    nodes.Add(new Node(l.Target.ToString(), l.Source.ToString()));



            }

            //Cria as conexões
            foreach (Node node in nodes)
            {
                List<ILink<T>> listaLinks = links.Where(p => p.Target.Equals(node.ToString()) || p.Source.Equals(node.ToString())).ToList();
                foreach (ILink<T> l in listaLinks)
                {
                    if (node.Conexoes.FindIndex(p => p.ToString().Equals(l.Source)) < 0 && !l.Source.Equals(node.ToString()))
                        node.Conexoes.Add(nodes.Find(p => p.ToString().Equals(l.Source.ToString())));

                    if (node.Conexoes.FindIndex(p => p.ToString().Equals(l.Target)) < 0 && !l.Target.Equals(node.ToString()))
                        node.Conexoes.Add(nodes.Find(p => p.ToString().Equals(l.Target.ToString())));

                }

            }

            //Cria as rotas

            //for(int i=0;i<=points.Count;i++)
            //{
            //    List<string> list = getCaminho(source.ToString(), target.ToString(), i, nodes);
            //    if (list.Count > 0)
            //        routesBetween.Add(list);

            //}


            routesBetween = getCaminhos(source.ToString(), target.ToString(), nodes);


            string str = "";



            //caminho = getCaminhos(source.ToString(), target.ToString());



            return (IEnumerable<T>)nodes.AsEnumerable();


        }
        protected List<List<string>> getCaminhos(string source, string target, List<Node> nodes)
        {
            List<List<string>> paths = new List<List<string>>();
            List<List<string>> rotas = new List<List<string>>();
            Dictionary<string, List<string>> dicVisiteds = new Dictionary<string, List<string>>();

            #region velho

            //Node node = nodes.Where(p => p.ToString().Equals(source)).ToList()[0];
            //if (node != null)
            //{
            //    foreach (Node conexao in node.Conexoes)
            //    {
            //        paths.Add(new List<string>());
            //        paths[node.Conexoes.IndexOf(conexao)].Add(conexao.ToString());

            //        Node n1 = nodes.First(p => p.ToString().Equals(conexao.ToString()));

            //        foreach (Node n in n1.Conexoes)
            //        {
            //            if(!n.ToString().Equals(source))
            //                paths[node.Conexoes.IndexOf(conexao)].Add(n.ToString());
            //        }
            //        for (int i = 0; i < paths.Count; i++)
            //        {
            //            foreach (List<string> path in paths)
            //            {
            //                Node n2 = nodes.First(p => p.ToString().Equals(path[i]));
            //                foreach (Node n in n2.Conexoes)
            //                {
            //                    if (!n2.ToString().Equals(source))
            //                        paths[node.Conexoes.IndexOf(conexao)].Add(n.ToString());
            //                }

            //                //Here continue


            //                //if (path.Contains(target))
            //                //{
            //                //    rotas.Add(path);
            //                //}
            //            }
            //        }
            //    }

            //}
            #endregion
            List<string> path = new List<string>();
            string next = source;
            while (true)
            {
                Node node = nodes.FirstOrDefault(p => p.ToString().Equals(next));
                node.Visited = true;

                if ((path.Contains(target) || path.Contains(source)) && !rotas.Contains(path))
                {
                    rotas.Add(path);

                    path = new List<string>();
                    foreach (Node n in nodes)
                    {
                        n.Visited = false;
                    }

                }
                foreach (Node nextNode in node.Conexoes)
                {
                    if (jaExisteSequencia(nextNode.ToString(), rotas, path)) continue;
                    if (!nextNode.Visited && !path.Contains(nextNode.ToString()) && !nextNode.ToString().Equals(source))
                    {
                        path.Add(nextNode.ToString());
                        nextNode.Visited = true;
                        next = nextNode.ToString();

                        break;
                    }
                }
            }

            return rotas;

        }

        private bool jaExisteSequencia(string conexaoAtual, List<List<string>> rotas, List<string> caminhoAtual)
        {
            if (rotas.Count == 0 || caminhoAtual.Count == 0) return false;
            List<string> newList = caminhoAtual;
            newList.Add(conexaoAtual);

            foreach (List<string> path in rotas)
            {
                List<string> listCompare = new List<string>();
                listCompare = DeepClonerExtensions.DeepClone(path);
                listCompare.Add(conexaoAtual);

                if (listCompare.SequenceEqual(newList)) return true;

                //foreach (string str in path)
                //{
                //    if (!str.Equals(conexaoAtual))
                //        return false;
                //}
            }
            return false;
        }
        //public static T DeepCopy<T>(T item)
        //{
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    MemoryStream stream = new MemoryStream();
        //    formatter.Serialize(stream, item);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    T result = (T)formatter.Deserialize(stream);
        //    stream.Close();
        //    return result;
            
        //}

        private List<string> getCaminho(string point, string target, int rota, List<Node> nodes)
        {
            try
            {
                bool repeat = false;
                bool found = false;
                List<string> lista = new List<string>();

                while (!repeat && !found)
                {
                    Node node = nodes.ToList().FirstOrDefault(p => p.ToString().Equals(point));
                    Node n = node.Conexoes[rota];

                    Node nextNode;
                    if (lista.Count == 0)
                        nextNode = nodes.FirstOrDefault(p => p.ToString().Equals(n.ToString()));
                    else
                        nextNode = nodes.FirstOrDefault(p => p.ToString().Equals(lista[lista.Count - 1]));

                    lista.Add(nextNode.Conexoes[rota].ToString());

                    if (nextNode.Conexoes[rota].ToString().Equals(point)) repeat = true;
                    if (nextNode.Conexoes[rota].ToString().Equals(target)) found = true;


                }
                return lista;
            }
            catch
            {
                return new List<string>();
            }
        }
        protected List<string> getNodes(List<string> points)
        {

            string path = "";
            List<string> paths = new List<string>();

            foreach (string point in points)
            {
                path = point;

                foreach (ILink<T> l in this.links)
                {

                    if (point.ToString().Contains(l.Source.ToString()))
                    {
                        if (!path.ToString().Contains(l.Target.ToString()))
                        {
                            path += "-" + l.Target.ToString();

                        }
                    }
                    else if (point.ToString().Contains(l.Target.ToString()))
                    {
                        if (!path.ToString().Contains(l.Source.ToString()))
                        {
                            path += "-" + l.Source.ToString();

                        }
                    }

                }
                paths.Add(path);

            }

            return paths;

        }


    }

    public class Node
    {
        string node;
        List<List<Node>> rotas = new List<List<Node>>();
        List<Node> conexoes = new List<Node>();
        bool visited = false;

        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }

        public List<Node> Conexoes
        {
            get { return conexoes; }
            set { conexoes = value; }
        }
        public Node(string node)
        {
            this.node = node;

        }

        public Node(string node, List<Node> nodes)
        {
            this.node = node;
            this.conexoes = nodes;

        }
        public Node(string node, string node1)
        {
            this.node = node;
            this.conexoes.Add(new Node(node1));

        }
        public Node(string node, string node1, string node2)
        {
            this.node = node;
            this.conexoes.Add(new Node(node1));
            this.conexoes.Add(new Node(node2));

        }
        public List<string> GetConexoes()
        {
            return conexoes.Select(p => p.ToString()).ToList();

        }

        public List<List<Node>> Rotas
        {
            get { return rotas; }
            set { rotas = value; }
        }
        public override string ToString()
        {
            return node.ToString();

        }


    }
}
namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }
        public static bool Equals(this List<string> lista, List<string> listaCompare)
        {
            foreach (string str in lista)
            {
                if (str.Equals(listaCompare[lista.IndexOf(str)]))
                    return true;

            }
            return false;

        }
        public static bool Contains(this List<List<string>> rotas, List<string> listaCompare)
        {
            foreach (List<string> l in rotas)
            {
                if (l.Equals(listaCompare))
                    return true;

            }
            return false;

        }


    }
}
