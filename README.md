# GamblingApp
CSCI 3110 Term Project



Project Overview

This application is a fake credit gambling website built as the final project for CSCI 3110. The goal was to design a complete web application with user accounts, database driven features, and interactive client side behavior. In this app, users can register, log in, manage their account information, and play two casino style games: Slots and Blackjack. Each game affects the user’s demo balance, and every bet is logged so the system always has a clear record of wins, losses, and wallet changes.
The project also includes a simple payment method system to simulate depositing credits into a user’s demo wallet. Deposits, bets, payouts, and admin adjustments all generate transactions, giving the application a consistent and traceable history of activity.
In addition to player functionality, the application includes an Admin Dashboard, which allows administrative users to view all accounts, update user information, apply credits, and review transaction logs for any account. The intention was to show how different permission levels interact with the same system and how user management fits into a database backed application.
Overall, the focus of the project was to build a functional web app that demonstrates full CRUD operations, REST API endpoints, many to one and many to many relationships, DOM driven UI interaction, and a structured backend using Controllers, Repositories, and Entity Framework Core.

Key Features to Note
• Admin Dashboard: View all users, adjust balances, toggle admin status, change emails, and review full transaction logs.
• Account Management: Users can update email/password, add/remove payment methods, and deposit demo credits.
• Gambling Games:
 Slots with reel visualization and a payout table
 Blackjack with server generated hands and bet validation
• Transaction Logging: Every deposit, bet, win/loss, and admin credit is saved to the database for traceability.
• Favorites System: Users can mark games as favorites, stored via a many to many relationship.

User Stories
The app was built around a set of user stories describing how players and admins interact with the system. These stories guided features such as registering and logging in, managing account details, placing bets, depositing funds, favoriting games, and allowing an admin to oversee system activity. Each major action a user or admin takes is supported by an API endpoint and reflected in the UI, ensuring the system behaves the way the user stories described.
Future Improvements
One improvement planned for later is implementing proper hashed/encrypted passwords instead of storing plain text, and learning the full hashing and verification process.



Running the Application 


dotnet build 

dotnet ef database update 

dotnet run 

After running these commands the application should create and seed the database. 
After opening the application on localhost, the user can either create a new account through the register button or log into an existing account. 
A professor account is already in the Dbseeder file, which is marked as an admin, to see non admin functionality create a new account. 


Professor account log-in:
- 
Email – drroach@gmail.com
Password – professor
-


Ai Disclosure: 

For this project, I used ChatGPT as an AI tool to help me learn and understand different parts of the assignment as I was building the application. AI wasn’t used to generate full solutions or write the project for me. Instead, it acted more like a fast reference tool that helped me figure out what I needed to understand so I could implement the correct solution myself.
Most of the help came from breaking down confusing errors, explaining how certain features in ASP.NET Core or JavaScript worked, and giving template-style examples so I could see the general structure of something before writing my own version. This sped up my debugging a lot since I didn’t have to spend hours searching for what an error might mean. It also helped me understand how the frontend and backend communicate, especially when wiring up API calls or handling state in JavaScript.
ChatGPT also helped me write documentation and summaries for classes, methods, and services. I wrote the code myself, and then asked ChatGPT to help phrase or tighten up the XML summaries so they were clear and matched what the method actually did. This helped me keep my documentation consistent and readable without changing the logic or design of the code.
The biggest insight I gained from using AI was with JavaScript. Before this class I had never written any JS, so having explanations and debugging help made it possible to build out the interactive parts of the site account management, the admin dashboard, the games, and the transaction updates without losing entire days stuck on one issue.
AI did not write any full components of this project. It didn’t generate controllers, repositories, services, or game logic. It never produced full code sections that I copied in. It mainly sped up the research process, helped me understand why something was breaking, and made it possible for me to take on a bigger project within the timeline. Everything in the final project the logic, design decisions, structure, and implementation was written and completed by me.
-
