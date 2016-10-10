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
                //------displays login and reg forms-----//
                return View["index.sshtml"];    
            }); 

            Post("/register", args =>            
            { 
                //-------storing user input-------//
                string first_name = Request.Form["first_name"];
                string last_name = Request.Form["last_name"]; 
                string email = Request.Form["email"];  
                string password = Request.Form["password"];
                string confirm = Request.Form["confirm"];

                //------validating user input------//
                if(first_name.Length == 0)
                {
                    @ViewBag.first_name = true;
                }
                if(last_name.Length == 0)
                {
                    @ViewBag.last_name = true;
                }
                if(email.Length == 0) 
                {
                    @ViewBag.email = true;   
                }
                if(password.Length < 8)
                {
                    @ViewBag.password = true;
                }
                if(confirm.Length == 0)
                {
                    @ViewBag.confirm = true;
                }
                else if(confirm != password)
                {
                    @ViewBag.match = true;
                }
                //-------if user input passes all validations-------//
                if(first_name.Length > 0 && last_name.Length > 0 && email.Length > 0 && password.Length > 7 && password == confirm)
                {
                    //---------store the input in the database----------//
                    string hash = Crypto.HashPassword(password);
                    string query = $"INSERT INTO users (first_name, last_name, email, hash, created_at) VALUES('{first_name}', '{last_name}', '{email}', '{hash}', NOW())";
                    DbConnector.ExecuteQuery(query);

                    //--------query the data again in descending order so the newest user is the first result-------//
                    query = "SELECT * FROM users ORDER BY id DESC LIMIT 1";
                    List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                    //---set an object to be == first user that was returned from the query (should be the only user that was returned)---//
                    Dictionary<string, object> new_user = user[0];

                    //--------store the user's unique id in session--------//
                    Session["current_user"] = (int)new_user["id"];
                    Console.WriteLine(Session["current_user"]);
                    
                    //-------redirect the new user to the success page-------//
                    return Response.AsRedirect("/users"); 
                }
                else
                {
                    //-------if there are errors, redirect the user to "/"-------//
                    return View["index.sshtml"];       
                }
            });  
 
            Post("/login", args => 
            {
                string email = Request.Form["email"];
                string password = Request.Form["password"];

                //------query the db to find the user that matches the email that the user inputs-----//
                string query = $"SELECT * FROM users WHERE email = '{email}' LIMIT 1";

                List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                //-------if no user was returned from the query, redirect to "/" with errors-------//
                if(user.Count == 0)
                {
                    @ViewBag.noUser = true;
                    return View["index.sshtml"];
                }
                //-------if no password was input, redirect to "/" with errors-------//
                if(password.Length == 0)
                {
                    @ViewBag.noPass = true;
                    return View["index.sshtml"];
                }
                else                       
                {
                    //---set an object to be == first user that was returned from the query (should be the only user that was returned)---//
                    Dictionary<string, object> match_user = user[0];

                    //------verify that the password given matches the hashed version of the password associated with the found user------//
                    bool match = Crypto.VerifyHashedPassword((string)match_user["hash"], password);

                    //------if the passwords match------//
                    if(match)
                    {
                        //------store the user's unique id in session & redirect to success page------//
                        Session["current_user"] = (int)match_user["id"];
                        return Response.AsRedirect("/users");  
                    }
                    //------if the passwords do not match, redirect to "/" with errors------//
                    else{
                        
                        @ViewBag.wrongPass = true;
                        return View["index.sshtml"]; 
                    }
                }
            });  

            Get("/users", args =>         
            {
                //---------displaying all users in the db----------//

                //------------create an empty view bag-------------//
                @ViewBag.users = ""; 

                //--------set an empty list to be == all users returned from querying the db--------//
                List<Dictionary<string, object>> results = DbConnector.ExecuteQuery("SELECT * FROM users");

                //---------reverse the list so that we can display the newest user on top-----------//
                results.Reverse();

                //------loop through the list of users and append each of their table data to the empty view bag------//
                foreach(Dictionary<string,object> item in results)
                {
                    @ViewBag.users += "<p>" + "<b>" + item["first_name"] + " " + item["last_name"] + "</b>" + " " + "registered at " + item["created_at"] + "</p>" + "<hr>";
                }

                //-----query the db to find the first name whose id matches the current user's id who is in session-----//
                string query = $"SELECT first_name FROM users WHERE id = {Session["current_user"]} LIMIT 1";

                //-----set an empty list to be == the user that is returned from the above query-----//
                List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                //-----loop through the list(will only loop once because the list only contains one user)-----//
                foreach(Dictionary<string,object> item in user)
                {
                    //-----set a view bag == the first name of the user-----//
                    //-----feel free to set more view bags for the rest of the user's data if you want to display it in the views-----//
                    @ViewBag.current_user = item["first_name"];
                }

                //-----now that we have all the user data we need to display in our views, we can render the users template-----//
                return View["users.sshtml"]; 
            }); 

            Post("/logout", args => 
            {
                //-----this route will delete all current sesssions and redirect to "/"-----//
                Session.DeleteAll();
                return Response.AsRedirect("/"); 
            });
        }
    }
}