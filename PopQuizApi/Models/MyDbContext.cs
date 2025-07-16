using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PopQuizApi.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameProgress> GameProgresses { get; set; }

    public virtual DbSet<GameResult> GameResults { get; set; }

    public virtual DbSet<GameTask> GameTasks { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

        var connectionString = configuration.GetConnectionString("MyDbConnection");
        optionsBuilder.UseSqlServer(connectionString);

    }
  

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("games");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DomainUrl).HasColumnName("domain_url");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UniqueId).HasColumnName("unique_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Games)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_games_users");
        });

        modelBuilder.Entity<GameProgress>(entity =>
        {
            entity.ToTable("game_progress");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Correct).HasColumnName("correct");
            entity.Property(e => e.Datetime)
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            entity.Property(e => e.GametaskId).HasColumnName("gametask_id");
            entity.Property(e => e.ParticipantId).HasColumnName("participant_id");
            entity.Property(e => e.SelectedAnswer).HasColumnName("selected_answer");

            entity.HasOne(d => d.Gametask).WithMany(p => p.GameProgresses)
                .HasForeignKey(d => d.GametaskId)
                .HasConstraintName("FK_game_progress_game_task");

            entity.HasOne(d => d.Participant).WithMany(p => p.GameProgresses)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK_game_progress_participants");
        });

        modelBuilder.Entity<GameResult>(entity =>
        {
            entity.ToTable("game_result");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Datetime)
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            entity.Property(e => e.ParticipantId).HasColumnName("participant_id");
            entity.Property(e => e.Ranking).HasColumnName("ranking");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Participant).WithMany(p => p.GameResults)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK_game_result_participants");
        });

        modelBuilder.Entity<GameTask>(entity =>
        {
            entity.ToTable("game_task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.OptionA).HasColumnName("optionA");
            entity.Property(e => e.OptionB).HasColumnName("optionB");
            entity.Property(e => e.OptionC).HasColumnName("optionC");
            entity.Property(e => e.OptionD).HasColumnName("optionD");
            entity.Property(e => e.Question).HasColumnName("question");
            entity.Property(e => e.QuestionNo).HasColumnName("question_no");

            entity.HasOne(d => d.Game).WithMany(p => p.GameTasks)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_game_task_games");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("participants");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.JoinTime)
                .HasColumnType("datetime")
                .HasColumnName("join_time");
            entity.Property(e => e.ParticipantName).HasColumnName("participant_name");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Game).WithMany(p => p.Participants)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_participants_games");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.RememberToken).HasColumnName("remember_token");
            entity.Property(e => e.TokenExpiredAt)
                .HasColumnType("datetime")
                .HasColumnName("token_expired_at");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
