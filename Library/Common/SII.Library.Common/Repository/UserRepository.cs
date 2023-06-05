using Agro.LibCore;
using Agro.Library.Model;
using System;

namespace Agro.Library.Common.Repository
{
	public class UserRepository: LibCore.Repository.CrudRepository<SEC_ID_USER >
	{
		private readonly IWorkspace _db;
		public UserRepository(IWorkspace db) { _db = db; }

		public override IWorkspace Db => _db;
		//public static UserRepository Instance { get; } = new UserRepository();

		public SEC_ID_USER  FindByUserName(string userName)
		{
			var user=base.Find(u =>u.Name==userName);
			if ((user == null||!user.Buildin) && userName == "admin")
			{
				RepairAdminUser(user);
				if (user == null)
				{
					user = Find(u => u.Name == userName);
				}
			}
			return user;
		}
		public bool IsPasswordOK(SEC_ID_USER  user, string pwd)
		{
			var sPwd = user.Password;
			var prefx = "{noop}";
            try
            {
                if (sPwd.StartsWith(prefx))
                {
                    sPwd = sPwd.Substring(prefx.Length);
                    return pwd == sPwd;
                }
                prefx = "{bcrypt}";
                if (sPwd.StartsWith(prefx))
                {
                    sPwd = sPwd.Substring(prefx.Length);
                    return BCrypt.Net.BCrypt.Verify(pwd, sPwd);
                }
                else
                {
                    return UserPasswordChecker.VerifyHashedPassword(sPwd, pwd);
                }
            }catch(Exception ex)
            {
                return false;
            }
		}

		private void RepairAdminUser(SEC_ID_USER  adminUser)
		{
			if (adminUser == null)
			{
				Insert(new SEC_ID_USER ()
				{
					ID = "U_CSADMIN",
					Name = "admin",
					Password = "{noop}admin@123456",
					Buildin = true,
				});
			}
			else if (!adminUser.Buildin)
			{
				adminUser.Buildin = true;
				Update(adminUser, u => u.ID == adminUser.ID, SEC_ID_USER.GetSubFields((c, t) => c(t.Buildin)));// new string[] { nameof(SEC_ID_USER.Buildin) });
			}
		}
	}

	class UserPasswordChecker
	{
		private const int PBKDF2IterCount = 185000; // default for Rfc2898DeriveBytes
		private const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
		private const int SaltSize = 128 / 8; // 128 bits

		// Define other methods and classes here
		public static bool VerifyHashedPassword(string hashedPassword, string password)
		{
			if (hashedPassword == null)
			{
				return false;
			}

			if (password == null)
			{
				throw new ArgumentNullException("password");
			}

			var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

			// Verify a version 0 (see comment above) text hash.

			if (hashedPasswordBytes.Length != (1 + SaltSize + PBKDF2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
			{
				// Wrong length or version header.
				return false;
			}

			var salt = new byte[SaltSize];
			Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
			var storedSubkey = new byte[PBKDF2SubkeyLength];
			Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, PBKDF2SubkeyLength);

			byte[] generatedSubkey;
			using (var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, PBKDF2IterCount))
			{
				generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
			}
			//storedSubkey.Dump("Stored");
			//generatedSubkey.Dump("Provided");
			return ByteArraysEqual(storedSubkey, generatedSubkey);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
		private static bool ByteArraysEqual(byte[] a, byte[] b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (a == null || b == null || a.Length != b.Length)
			{
				return false;
			}

			var areSame = true;
			for (var i = 0; i < a.Length; i++)
			{
				areSame &= (a[i] == b[i]);
			}
			return areSame;
		}
	}
}
