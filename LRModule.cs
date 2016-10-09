using System;
using System.Collections.Generic;
using Nancy;
using DbConnection;
using CryptoHelper;
 
namespace Login_Reg_Nancy   
{
    public class LRModule : NancyModule  
    {
        public LRModule()   
        {
            Get("/", args =>
            {
                return View["index.sshtml"];    
            }); 

            Post("/register", args =>            
            {
                Console.WriteLine("Creating A New User");  

                string first_name = Request.Form["first_name"];
                
                string last_name = Request.Form["last_name"]; 
                
                string email = Request.Form["email"];   

                string password = Request.Form["password"];
                string hash = Crypto.HashPassword(password); 

                if(first_name.Length < 2)
                {
                    @ViewBag.first_name = true;
                }
                if(last_name.Length < 2)
                {
                    @ViewBag.last_name = true;
                }
                if(email.Length < 2)
                {
                    @ViewBag.email = true;
                }
                if(password.Length < 8)
                {
                    @ViewBag.password = true;
                }
                if(first_name.Length > 2 && last_name.Length > 2 && email.Length > 6 && password.Length >= 8)
                {
                    string query = $"INSERT INTO users (first_name, last_name, email, hash, created_at) VALUES('{first_name}', '{last_name}', '{email}', '{hash}', NOW())";
                    DbConnector.ExecuteQuery(query);
                    return Response.AsRedirect("/users"); 
                }
                else
                {
                    return View["index.sshtml"];      
                }
            });  
 
            Post("/login", args => 
            {
                //verify if email matches any emails in db
                //if an email is found, the user exists
                // string email = Request.Form["email"];
                // List<Dictionary<string, object>> user = DbConnector.ExecuteQuery($"SELECT * FROM users WHERE email = '{email}'");
                
                return Response.AsRedirect("/users");     
            });  

            Get("/users", args =>         
            {
                @ViewBag.users = ""; 

                List<Dictionary<string, object>> results = DbConnector.ExecuteQuery("SELECT * FROM users");
                results.Reverse();
                foreach(Dictionary<string,object> item in results)
                {
                    @ViewBag.users += "<p>" + "<b>" + item["first_name"] + " " + item["last_name"] + "</b>" + " " + "registered at " + item["created_at"] + "</p>" + "<hr>";
                }
                return View["users.sshtml", results]; 
            }); 

            Post("/logout", args => 
            {
                // Session.DeleteAll();
                return Response.AsRedirect("/"); 
            });
        }
    }
}