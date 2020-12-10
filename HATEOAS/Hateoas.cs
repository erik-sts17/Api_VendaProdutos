using System.Collections.Generic;

namespace Desafio_Api.HATEOAS

{
    public class Hateoas
    {
        public string _url;
        public string _protocol = "https://";
        public List<Link> actions = new List<Link>();

        public Hateoas(string url)
        {
            _url = url;
        }

        public Hateoas(string protocol, string url)
        {
            _protocol = protocol;
            _url = url;
        }

        public void AddAction(string rel, string method)
        {
            actions.Add(new Link(_protocol + _url, rel, method));
        }

        public Link[] GetActions(string sufix)
        {
           Link[] tempLinks = new Link[actions.Count];

           for (int i = 0; i < tempLinks.Length; i++)
           {
               tempLinks[i] = new Link(actions[i].Href, actions[i].Rel, actions[i].Method);
           }

           foreach (var link in tempLinks)
           {
               link.Href = link.Href + "/" + sufix;
           }
           return tempLinks;
        }
    }
}