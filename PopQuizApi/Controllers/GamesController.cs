using com.google.zxing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PopQuizApi.Models;
using PopQuizApi.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;


namespace PopQuizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public GamesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Games
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Game>>> GetGames()
        //{
        //    return await _context.Games.ToListAsync();
        //}


        [HttpGet("get")]
        public async Task<ActionResult<List<Game>>> GetGameByUser([FromHeader] string token )
        {

            if (!await AuthService.validateUser(HttpContext))
            {
                return Unauthorized();
            }

            var user = HttpContext.Items["User"] as User;
            var game = _context.Games.Where(x => x.UserId == user.Id).ToList();

            return Ok(game.Select(x => new { x.Title, x.Status, x.StartTime, x.EndTime, x.Description, x.UniqueId }).ToList());
            // return game;
        }

        // GET: api/Games/5
        [HttpGet("{unique_id}")]
        public  ActionResult<Bitmap> GetGame(string unique_id)
        {
            try
            {
                var game = _context.Games.ToList().FirstOrDefault(x => x.UniqueId == unique_id && x.StartTime <= DateTime.Now && x.EndTime > DateTime.Now);

                if (game == null)
                {
                    return NotFound();
                }

                var qr = new QrService();
                string host = game.DomainUrl ?? Request.Scheme + "://" + Request.Host.Value + "/games/join";
                string endpoint = $"/{game.UniqueId}";
                string qrText = $"{host}{endpoint}";
                Image<Rgba32> bitmap = qr.GenerateQRCode(qrText);


                using var ms = new MemoryStream();
                bitmap.Save(ms, new PngEncoder());
                ms.Seek(0, SeekOrigin.Begin);

                return File(ms.ToArray(), "image/png");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
           
            // return game;
        }

        // GET: api/Games/5

        private List<RankingResult> getRankingUsingSession(string session, bool all = true)
        {

            var game = _context.Participants.FirstOrDefault(x => x.SessionId == session);

            var participants = _context.Participants.Where(x => x.GameId == game.GameId).Include(x => x.Game).Include(x => x.GameProgresses).ToList();
            var rawranking = participants.Select(x => new RankingResult
            {
                ParticipantName = x.ParticipantName,
                Progress = x.GameProgresses.Count,
                CorrectCount = x.GameProgresses.Count(x => x.Correct == true),
                Accuracy = x.GameProgresses.Any()
                            ? (double)x.GameProgresses.Count(x => x.Correct == true) / x.GameProgresses.Count :
                            0,
                Completed = x.GameProgresses.Count >= x.Game.GameTasks.Count,
                Time = x.GameProgresses.Any(x => x.Datetime != null) ? (x.GameProgresses.Max(x => x.Datetime) - x.GameProgresses.Min(x => x.Datetime)).Value.TotalSeconds : int.MaxValue

            }).OrderByDescending(x => x.CorrectCount).ThenByDescending(x => x.Accuracy).ThenBy(x => x.Time).ToList();

            var ranking = rawranking.Select(x =>  { var rank = x;
                rank.ranking = rawranking.IndexOf(x) + 1;
                return rank;
            }).ToList(); 

            if (all)
            {

                return ranking;
            }
            else
            {
                return ranking.Where(x=>x.ParticipantName == game?.ParticipantName).ToList();

            }
        }


        private List<RankingResult> getRankingUsingToken(string id)
        {

            var games = _context.Games.FirstOrDefault(x => x.UniqueId == id);

            var participants = _context.Participants.Where(x => x.GameId == games.Id).Include(x => x.Game).Include(x => x.GameProgresses).ToList();
            var ranking = participants.Select(x =>  new RankingResult
            {
                ParticipantName =  x.ParticipantName,
                Progress = x.GameProgresses.Count,
                CorrectCount = x.GameProgresses.Count(x => x.Correct == true),
                Accuracy = x.GameProgresses.Any()
                            ? (double)x.GameProgresses.Count(x => x.Correct == true) / x.GameProgresses.Count :
                            0,
                Completed = x.GameProgresses.Count >= x.Game.GameTasks.Count,
                Time = x.GameProgresses.Any(x => x.Datetime != null) ? (x.GameProgresses.Max(x => x.Datetime) - x.GameProgresses.Min(x => x.Datetime)).Value.TotalSeconds : int.MaxValue

            }).OrderByDescending(x => x.CorrectCount).ThenByDescending(x => x.Accuracy).ThenBy(x => x.Time).ToList();

            return ranking;
        }

        [HttpGet("result")]
        public async Task<ActionResult> getResult([FromHeader] string? token, [FromQuery]string? id, [FromQuery] bool all)
        {

            if(string.IsNullOrEmpty(token + id))
            {
                return BadRequest("Game Unique Id or Participant Session id is required");
            }
            if (string.IsNullOrEmpty(id))
            {
                var ranking = getRankingUsingSession(token,all);

                return Ok(ranking);
            }
            else
            {

                if (!await AuthService.validateUser(HttpContext))
                {
                    return Unauthorized();
                }

                var ranking = getRankingUsingToken(id);
                return Ok(ranking);
             
            }
          
        }

        // GET: api/Games/5
        [HttpPost("join")]
        public async Task<ActionResult<Participant>> join([FromBody] Participant participant)
        {

            var game = _context.Games.ToList().Where(x => x.UniqueId == participant.SessionId).FirstOrDefault();

            if (game == null)
            {
                return NotFound();
            }

            if(_context.Participants.Any(x=>x.GameId == game.Id && x.ParticipantName == participant.ParticipantName))
            {
                return BadRequest("This username have been used in this game");
            }
            var charlist = Enumerable.Range(0, 26).Select(x => (char)('A' + x)).ToList();
            var numberlist = Enumerable.Range(0, 10).Select(x => x.ToString()).ToList();

            Random rand = new Random();
            var list = string.Join("", charlist) + string.Join("", charlist.Select(x => char.ToLower(x))) + string.Join("", numberlist);
            var session_id =participant.ParticipantName + "_"+ string.Join("", Enumerable.Range(0, 20).Select(x => list[rand.Next(0, list.Count() - 1)])) + DateTime.Now.ToString("yyyyhhddmmssMM");
            var newparticipant = new Participant()
            {
                GameId = game.Id,
                ParticipantName = participant.ParticipantName,
                SessionId = session_id,
                Status = "Enrolled",
                JoinTime = DateTime.Now,
                

            };

            _context.Add(newparticipant);
            _context.SaveChanges();

            newparticipant.Game = null;
            newparticipant.GameId = 0;
            newparticipant.GameProgresses = null;
            newparticipant.Id = 0;
            

            return Ok(newparticipant);


            // return game;
        }


        //        {
        //  "title": "string",
        //  "startTime": "2025-06-16T13:45:53.935Z",
        //  "endTime": "2025-06-16T13:55:53.935Z",
        //  "description": "string",
        //  "domainUrl": "string",
        //  "gameTasks": [
        //    {

        //      "question": "Who is Admin",
        //      "optionA": "Mingxuan",
        //      "optionB": "Zikri",
        //      "optionC": "Haziq",
        //      "optionD": "Khairul",
        //      "answer": "Mingxuan",


        //    },|
        //{

        //      "question": "What is answer of the question",
        //      "optionA": "A",
        //      "optionB": "B",
        //      "optionC": "C",
        //      "optionD": "D",
        //      "answer": "B",

        //    }
        //  ]
        //}
        [HttpGet ("socket")]
        public async Task<ActionResult> check([FromHeader] string token)
        {
            var participant = _context.Participants.Include(p=>p.Game).FirstOrDefault(x => x.SessionId == token);

            if(participant == null)
            {
                return Unauthorized();
            }

           await  SocketService.sendUpdate(participant.Game);

            return Ok("success");
        }

        
        // POST: api/Games
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame(Game game, [FromHeader] string token)
        {
            try
            {
                if (!await AuthService.validateUser(HttpContext))
                {
                    return Unauthorized();
                }

                var user = HttpContext.Items["User"] as User;



                if (string.IsNullOrEmpty(game.Title) || game.StartTime == null || game.GameTasks.Count <= 0)
                {
                    return BadRequest("Invalid Game Data");
                }

                if (_context.Games.Any(x => x.Title == game.Title && user.Id == x.UserId))
                {
                    return BadRequest("You already have game with same name");

                }

                if (!string.IsNullOrEmpty(game.DomainUrl))
                {
                    if (!game.DomainUrl.StartsWith("http://") && !game.DomainUrl.StartsWith("https://"))
                    {
                        return BadRequest("Url should start with http or https protocol");
                    }


                    if (!Uri.TryCreate(game.DomainUrl, UriKind.Absolute, out var res))
                    {
                        return BadRequest("Invalid url format");
                    }
                    //var regex = @"^(http:\/\/|https:\/\/)([A-Za-z0-9]+\.)+([A-Za-z0-9]+)(:[0-9]+)?(\/.*)?$";

                    //if (!Regex.IsMatch(game.DomainUrl, regex))
                    //{
                    //    return BadRequest("Invalid url format");
                    //}

                    if (game.DomainUrl.EndsWith("/"))
                    {
                        game.DomainUrl = game.DomainUrl.Remove(game.DomainUrl.Length - 1);
                    }
                }
                game.User = null;
                game.UserId = user.Id;

                game.Status = true;

                var charlist = Enumerable.Range(0, 26).Select(x => (char)('A' + x)).ToList();
                var numberlist = Enumerable.Range(0, 10).Select(x => x.ToString()).ToList();


                var list = string.Join("", charlist) + string.Join("", charlist.Select(x => char.ToLower(x))) + string.Join("", numberlist);

                var rand = new Random();


                var unique_id = string.Join("", Enumerable.Range(0, 8).Select(x => list[rand.Next(0, list.Count() - 1)]));
                //var session_id = string.Join("", Enumerable.Range(0, 8).Select(x => list[rand.Next(0, list.Count() - 1)])) + DateTime.Now.ToString("hhmmssyyyyMMdd") + "-" + game.UniqueId;
                //
                int question_number = 1;
                foreach (var item in game.GameTasks)
                {
                    item.QuestionNo = question_number++;

                    if (item.Answer == item.OptionA)
                    {
                        continue;
                    }
                    if (item.Answer == item.OptionB)
                    {
                        continue;
                    }
                    if (item.Answer == item.OptionC)
                    {
                        continue;
                    }
                    if (item.Answer == item.OptionD)
                    {
                        continue;
                    }

                    return BadRequest($"Please validate the answer of question {question_number}");
                }

                _context.Games.Add(game);
                _context.SaveChanges();

                var g = _context.Games.Find(game.Id);
                g.UniqueId = unique_id + game.Id;
                _context.SaveChanges();


               
                return Ok(game.UniqueId);
            }
            catch (Exception ex)
            {

                return Conflict(ex.Message);
            }
           
        }

        //[HttpGet("admin-socket")]
        //// DELETE: api/Games/5

        //public async Task<IActionResult> AdminGameSession([FromHeader] string id)
        //{
        //    var session = _context.Participants.Include(x => x.Game).ThenInclude(x => x.GameTasks).Include(x => x.GameProgresses).FirstOrDefault(x => x.SessionId == id);

        //    if (session == null)
        //    {
        //        return Unauthorized();
        //    }
        //    var gametask = session.Game.GameTasks.OrderBy(x => x.QuestionNo).ToList();

        //    foreach (var g in gametask)
        //    {
        //        if (!session.GameProgresses.Any(x => x.GametaskId == g.Id))
        //        {
        //            g.Game = null;
        //            g.GameId = 0;
        //            g.GameProgresses = null;
        //            g.Answer = null;

        //            return Ok(g);
        //        }
        //    }

        //    var result = _context.GameResults.Where(x => x.ParticipantId == session.Id).FirstOrDefault();

        //    result.Participant = null;


        //    var list = _context.Participants.Where(x => x.GameId == session.GameId && x.Status == "Completed").Select(x => new { x.Id, correctness = x.GameProgresses.Count(y => y.Correct == true) }).OrderByDescending(x => x.correctness).ToList();

        //    var item = list.FirstOrDefault(x => x.Id == result.ParticipantId);
        //    result.Ranking = list.IndexOf(item ?? list.Last()) + 1;

        //    result.Result = $"You are the {result.Ranking}nd in the game {session.Game.Title} now";




        //    result.ParticipantId = null;

        //    return Ok(result);

        //}


        [HttpGet("session")]
        // DELETE: api/Games/5
      
        public async Task<IActionResult> GameSession([FromHeader]string id)
        {
            var session =  _context.Participants.Include(x=>x.Game).ThenInclude(x=>x.GameTasks).Include(x=>x.GameProgresses).FirstOrDefault(x => x.SessionId == id);

            if(session == null)
            {
                return Unauthorized();
            }
            var gametask = session.Game.GameTasks.OrderBy(x=>x.QuestionNo).ToList();

            foreach(var g in gametask)
            {
                if(!session.GameProgresses.Any(x=>x.GametaskId == g.Id))
                {
                    g.Game = null;
                    g.GameId = 0;
                    g.GameProgresses = null;
                    g.Answer = null;
                   
                    return Ok(g);
                }
            }

            var result = _context.GameResults.Where(x=>x.ParticipantId == session.Id).FirstOrDefault();

            result.Participant = null;
            

            var list = _context.Participants.Where(x => x.GameId == session.GameId && x.Status == "Completed").Select(x=> new {x.Id, correctness = x.GameProgresses.Count(y=>y.Correct == true)}).OrderByDescending(x => x.correctness).ToList();

            var item = list.FirstOrDefault(x => x.Id == result.ParticipantId);
            result.Ranking = list.IndexOf(item??list.Last()) + 1;

            result.Result = $"You are the {result.Ranking}nd in the game {session.Game.Title} now";




            result.ParticipantId = null;

            return Ok(result);
           
        }


        private string getDuration(double second)
        {
            TimeSpan time = TimeSpan.FromSeconds(second);
            List<string> parts = new();

            if (time.Hours > 0)
                parts.Add($"{time.Hours}H");
            if (time.Minutes > 0)
                parts.Add($"{time.Minutes}M");
            if (time.Seconds > 0 || parts.Count == 0)
                parts.Add($"{time.Seconds}S");

            return string.Join(" ", parts);
        }
        [HttpPost("answer")]
        // DELETE: api/Games/5

        public async Task<IActionResult> submitAnswer([FromBody] GameProgress progress, [FromHeader] string session)
        {


            var participant = _context.Participants.FirstOrDefault(x => x.SessionId == session);

            if( _context.GameProgresses.Any(x=>x.GametaskId == progress.GametaskId && x.ParticipantId == participant.Id))
            {
                return BadRequest("You have answered this question");
            }

            var question = _context.GameTasks.FirstOrDefault(x => x.Id == progress.GametaskId);
            var game = _context.Games.Include(x=>x.GameTasks).FirstOrDefault(x => x.Id == participant.GameId);


            if (question == null )
            {
                return BadRequest("Invalid Task Id");
            }

            if(progress.SelectedAnswer != question.OptionA && progress.SelectedAnswer != question.OptionB &&
                progress.SelectedAnswer != question.OptionC && progress.SelectedAnswer != question.OptionD)
            {
                return BadRequest("Invalid Answer");
            }

            progress.ParticipantId = participant.Id;
            progress.Datetime = DateTime.Now;
            progress.Correct = progress.SelectedAnswer == question.Answer;

            _context.GameProgresses.Add(progress);
            _context.SaveChanges();

            if(_context.GameProgresses.Count(x=>x.ParticipantId == participant.Id) >= game.GameTasks.Count)
            {
                if(!_context.GameResults.Any(x=>x.ParticipantId == participant.Id))
                {
                    participant.Status = "Completed";
                    var list = getRankingUsingSession(participant.SessionId);
                    var item = list.FirstOrDefault(x=>x.ParticipantName == participant.ParticipantName);
                    GameResult result = new GameResult() { Status = true,Result = $"Score: {item.Accuracy:0.00}% in {getDuration(item.Time)}" ,Datetime = DateTime.Now, ParticipantId = participant.Id};

                    _context.GameResults.Add(result);

                }
                else
                {
                    return BadRequest("Result Announced");
                }
                
                

            }
            else
            {
                participant.Status = "Answering";

            }

            _context.SaveChanges();
            return Ok("Result Saved");



        }



        private bool GameExists(long id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}
