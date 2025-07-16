<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\AuthController;
use App\Http\Controllers\QuizController;


Route::get('/', function () {
    return redirect()->route('welcome');});

Route::get('/signin', [AuthController::class, 'indexSignIn'])->name('signin-page');
Route::get('/signup', [AuthController::class, 'indexSignUp'])->name('signup-page');

//WELCOME/QUICKJOIN

Route::get('/welcome', [QuizController::class, 'showHomePage'])->name('welcome');  // New route for the welcome page
Route::post('/join-game-guest', [QuizController::class, 'joinGameGuest'])->name('join-game-guest');



// REGISTER USER
Route::post('/signup-process', [AuthController::class, 'signUpProcess'])->name('signup-user');

// LOGIN & LOGOUT USER
Route::post('/authentication', [AuthController::class, 'authentication'])->name('auth-user');
Route::get('/user-logout', [AuthController::class, 'userLogout'])->name('user-logout');



// USER CREATE QUIZ [INITIALLY HOME]
Route::get('/user-create-quiz', [AuthController::class, 'indexUserHome'])->name('user-home');

//CREATE QUIZ
Route::post('/create-quiz', [QuizController::class, 'createQuiz'])->name('create-quiz');
Route::get('/show-quiz-{uniqueId}', [QuizController::class, 'showQuiz'])->name('show-quiz');


Route::get('/join-game-{id}', [QuizController::class, 'joinGame'])->name('join-Game');



    Route::get('/join-game/{id}', [QuizController::class, 'showJoinForm'])
         ->name('join-game-form');

    // Submit Join → panggil API /api/Games/join
    Route::post('/join-game', [QuizController::class, 'joinGame'])
         ->name('join-game');

    // Tampilkan soal selanjutnya
    Route::get('/game-session/{sessionId}', [QuizController::class, 'showQuestion'])
         ->name('game-session');

    // Submit jawaban (stub; sesuaikan endpoint sebenarnya jika beda)
    Route::post('/game-session/{sessionId}', [QuizController::class, 'submitAnswer'])
         ->name('submit-answer');

         Route::get('/dashboard/{uniqueId}', [QuizController::class, 'adminDashboard'])
         ->name('admin.dashboard');

    /**
     * API endpoint for the JavaScript to fetch live results.
     * This corresponds to the getLiveResults() method and is named
     * as expected by the dashboard's script.
     */
    Route::get('/quiz/{uniqueId}/live-results', [QuizController::class, 'getLiveResults'])
         ->name('admin.getLiveResults');

