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

                if(first_name.Length > 2 && last_name.Length > 2 && email.Length > 6 && password.Length >= 8)
                {
                    string query = $"INSERT INTO users (first_name, last_name, email, hash, created_at) VALUES('{first_name}', '{last_name}', '{email}', '{hash}', NOW())";
                    DbConnector.ExecuteQuery(query);
                    @ViewBag.error = false;
                    return Response.AsRedirect("/users"); 
                }
                else
                {
                    @ViewBag.error = true;
                    return View["index.sshtml"];
                }
            });  

            Post("/login", args => 
            {
                // //verify email exists in db
                // string email = Request.Form["email"];
                // List<Dictionary<string, object>> result = DbConnector.ExecuteQuery($"SELECT * FROM users WHERE email = '{email}'");

                 //verify password is correct
                // string password = Request.Form["password"];
                // bool IsCorrectString = Crypto.VerifyHashedPassword(hash, password);

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