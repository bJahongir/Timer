using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
namespace Timer;

public class TimerRepostory
{
    string connectionString = "";

    public TimerRepostory(IConfiguration con)
    {
        connectionString = con.GetConnectionString("Postgres") ?? "";
    }

    public void Save(TimerRecord record)
    {
        string sqlCmd = @"
    INSERT INTO timerrecords(""start"", ""end"") 
    VALUES (@start, @end)";

        using NpgsqlConnection conn = new(connectionString);
        using NpgsqlCommand cmd = new(sqlCmd, conn);
        cmd.Parameters.AddWithValue("@start", record.Start.Value);
        cmd.Parameters.AddWithValue("@end", record.End.Value);
        conn.Open();
        cmd.ExecuteNonQuery();

    }

    public IEnumerable<TimerRecord> GetTimerHistory()
    {
        string sqlCmd = "select * from timerrecords order by start desc limit 10";
        using NpgsqlConnection conn = new(connectionString);
        using NpgsqlCommand cmd = new(sqlCmd, conn);
        cmd.CommandType = CommandType.Text;
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            yield return new TimerRecord
            {
                Id = reader.GetInt32("id"),
                Start = reader.GetDateTime("start"),
                End = reader.GetDateTime("end")
            };
        }
    }
}

public class TimerRecord
{
    public int Id { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
