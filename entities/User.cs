using LiteDB;

namespace Malaco5.Entities;

public class User{
    public ulong UserId { get; set; }
    public required Dictionary<string, string> savedRolls { get; set; }
    const string _db = "./db/data.db";

    public User SaveUser(){
        using (var db = new LiteDatabase(_db)){
            var collection = db.GetCollection<User>("users");
            collection.Insert(this);
            collection.EnsureIndex(x => x.UserId);
            return this;
        }
    }

    public static void EnsureUser(ulong userId){
        var user = GetUser(userId, out var e);
        if(!e)
            user.SaveUser();
    }

    public void UpdateUser(){
        using (var db = new LiteDatabase(_db)){
            var collection = db.GetCollection<User>("users");
            collection.Update(this);
        }
    }

    public static User GetUser(ulong userId, out bool existed){
        using (var db = new LiteDatabase(_db)){
            var collection = db.GetCollection<User>("users");
            var results = collection.Query().Where(x => x.UserId == userId);
            existed = (results.Count() != 0);
            if(!existed)
                return new User(){
                    UserId = userId,
                    savedRolls = new Dictionary<string, string>()
                };
            return results.First();
        }
    }
}