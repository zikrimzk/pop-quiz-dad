# 📚 POP QUIZ: Interactive Quiz System

## 🚀 Introduction

### 📌 Project Overview
**POP QUIZ** is a Kahoot-like interactive quiz system designed to make learning fun and engaging. This project enables hosts (teachers, trainers, or admins) to create quizzes and let participants join from both web and mobile platforms using either a QR code or a game code.

Supported functionalities:
- ✅ Quiz creation and management via web
- ✅ Quiz participation through web and mobile
- ✅ Real-time quizzes for classrooms, training sessions, or group learning

### 🔗 Commercial Value / Third-Party Integration
POP QUIZ uses the [GoQR.me API](https://goqr.me/api/) and [Zxing](https://www.nuget.org/packages/ZXing.Net/) to generate QR codes for each quiz. This allows participants to easily scan and join games, enhancing accessibility and user convenience.

Pop Quiz application targets Kementerian Pendidikan Tinggi (KPT) institutions and universities, revolutionizing interactive learning and assessment. While sharing the engaging spirit of platforms like Kahoot!, our key differentiator lies in its ability to facilitate asynchronous participation with real-time progress updates, offering unparalleled flexibility and richer insights into learning outcomes.

---

## 🧱 System Architecture

### 🧩 High-Level Diagram
![Alt text](https://github.com/zikrimzk/pop-quiz-dad/blob/main/images/system_architecture.png?raw=true)

---

## 🗃️ Database Design

### 🧾 ERD (Entity-Relationship Diagram)
![Alt text](https://github.com/zikrimzk/pop-quiz-dad/blob/main/images/erd.png?raw=true)

### 💡 Schema Justification

- `users`: Stores information about hosts or administrators who create and manage quizzes.
- `participants`: Represents users who join and participate in quizzes.
- `games`: Contains metadata for each quiz session, such as title, schedule, and description.
- `game_tasks`: Holds the individual quiz questions associated with each game.
- `game_progress`: Tracks participants’ real-time responses during a quiz.
- `game_result`: Stores the final score or outcome for each participant after completing a quiz.

**Key Relationships:**
- One-to-many: A single `user` can create multiple `games`.
- One-to-many: Each `game` can contain multiple `game_tasks` (questions).
- One-to-many: A `participant` can have multiple `game_progress` entries as they answer each question.
- One-to-many: A `participant` can have multiple `game_result` records for different games they joined.



---
## 🖥️ Backend Application

### ⚙️ Technology Stack
- **Language:** C#
- **Framework:** ASP.NET Core
- **Database:** Microsoft SQL Server (MSSQL)
- **Other:**  
  - Docker (for deployment and development)  
  - WebSockets (for real-time synchronization)  
  - GoQR.me API and zxing (QR code generation)

### 📘 API Documentation
Production Swagger Docs: [http://156.67.218.162:5000/swagger/index.html](http://156.67.218.162:5000/swagger/index.html)

#### 🔐 Authentication Endpoints

- **Register**
  ```
  POST /api/auth/register
  ```
  Example:
  ```json
  {
    "username": "admin",
    "password": "Abc!23",
    "email": "test@gmail.com"
  }
  ```

- **Login**
  ```
  POST /api/auth/login
  ```
  Example:
  ```json
  {
    "username": "admin",
    "password": "Abc!23"
  }
  ```

- **Validate Token**
  ```
  GET /api/auth/validate
  ```
  Header: `token: <your_token>`

---

#### 🧩 Game Management Endpoints

- **Create Game**
  ```
  POST /api/Games
  ```
  Headers:
  - `token: <your_token>`
  - `Content-Type: application/json`

  Example Request Body:
  ```json
  {
    "title": "API Quiz",
    "startTime": "2025-06-18T10:00:00Z",
    "endTime": "2025-06-18T10:20:00Z",
    "description": "Test your REST API knowledge",
    "domainUrl": "https://yourdomain/games/join/",
    "gameTasks": [
      {
        "question": "What does REST stand for?",
        "optionA": "Remote Execution Standard Transfer",
        "optionB": "Representational State Transfer",
        "optionC": "Reliable Endpoint Secure Transport",
        "optionD": "Request Event Session Transfer",
        "answer": "Representational State Transfer"
      }
    ]
  }
  ```

- **Get All Games**
  ```
  GET /api/Games/get
  ```

- **Get Game QR Code**
  ```
  GET /api/Games/{gameId}
  ```

---

#### 🧑‍💻 Participant Endpoints

- **Join Game**
  ```
  POST /api/Games/join
  ```
  ```json
  {
    "participantName": "student1",
    "sessionId": "game123"
  }
  ```

- **Get Questions**
  ```
  GET /api/Games/session
  ```
  Header: `id: <sessionId>`

- **Submit Answer**
  ```
  POST /api/Games/answer
  ```
  ```json
  {
    "gametaskId": 13,
    "selectedAnswer": "JSON"
  }
  ```

- **View Result (Participant)**
  ```
  GET /api/Games/result
  ```
  Header: `token: <participant_token>`

- **View Result (Admin)**
  ```
  GET /api/Games/result?id={gameId}
  ```
  Header: `token: <admin_token>`

- **WebSocket Connection**
  ```
  GET /games/connect
  ```

---

### 🔒 Security
- Token-based authentication
- Session validation via headers
- Role-based access (admin / user / participant)

---

## 💻 Frontend Applications

### 🌐 Web Application (Laravel)
- **Purpose:** For quiz hosts/admins
- **Stack:** Laravel (PHP)
- **Features:**
  - 📝 Host registration & login
  - ➕ Create/manage quizzes
  - 📤 Share via QR/game code
  - 📊 View results

🔗 Repo: [Laravel Frontend](https://github.com/zikrimzk/pop-quiz-dad/tree/main/laravel)

---

### 📱 Mobile Application (Flutter)
- **Purpose:** For quiz participants
- **Stack:** Flutter (Dart)
- **Features:**
  - 📷 Join via QR code
  - 🔢 Join with game code
  - 📊 View results

🔗 Repo: [Flutter Resources](https://github.com/zikrimzk/pop-quiz-dad/tree/main/flutter_mobile)

---

## 🧠 Business Logic & Validation
### ✅ Validation Rules

#### Registration
- Fields required: `username`, `email`, `password`
- Email must be unique
- Password must be:
  - At least 8 characters
  - Include 1 capital letter
  - Include 1 symbol

#### Login
- Fields required: `username`, `password`
- Show error for invalid credentials

#### Create Game
- Fields required: `title`, `start date`, `end date`
- Start date must not be in the past
- End date must be after start date

---

## 👨‍👩‍👧‍👦 Authors

| Name                                     | Matric No     |
|------------------------------------------|---------------|
| Chuah Ming Xuan                          | B032320027    |
| Muhammad Zikri Bin Kashim                | B032320063    |
| Iskandar Zulkanain Bin Rosmi             | B032320033    |
| Khairul Adzhar Bin Noraidi               | B032320036    |
| Muhammad Haziq Bin Fadhlan Faizal        | B032320052    |
| Muhammad Qiwamuddin Abqari Bin Norefendi | B032320057    |
| Wan Mohamad Irfan Bin Mohd Roslan        | B032320101    |

---

## 🎥 Demo Video
[📺 Watch on YouTube](https://www.youtube.com/watch?v=S4xIYNnN9cI)

---

## 📄 License
Academic project under Universiti Teknikal Malaysia Melaka (UTeM)  
Course: Distributed Application Development (BITP 3123)
