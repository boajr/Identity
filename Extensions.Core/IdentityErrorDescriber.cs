using Microsoft.AspNetCore.Identity;

namespace Boa.Identity
{
    public class IdentityErrorDescriber : Microsoft.AspNetCore.Identity.IdentityErrorDescriber
    {
        private readonly IdentityStringLocalizer _localizer;

        public IdentityErrorDescriber(IServiceProvider serviceProvider)
        {
            _localizer = new(serviceProvider, "Microsoft.AspNetCore.Identity.IdentityErrorDescriber");
        }



        /// <summary>
        /// Returns the default <see cref="IdentityError"/>.
        /// </summary>
        /// <returns>The default <see cref="IdentityError"/>.</returns>
        public override IdentityError DefaultError()
        {
            var str = _localizer["DefaultError"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.DefaultError();
            }

            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a concurrency failure.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a concurrency failure.</returns>
        public override IdentityError ConcurrencyFailure()
        {
            var str = _localizer["ConcurrencyFailure"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.ConcurrencyFailure();
            }

            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password mismatch.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password mismatch.</returns>
        public override IdentityError PasswordMismatch()
        {
            var str = _localizer["PasswordMismatch"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordMismatch();
            }

            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating an invalid token.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating an invalid token.</returns>
        public override IdentityError InvalidToken()
        {
            var str = _localizer["InvalidToken"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.InvalidToken();
            }

            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a recovery code was not redeemed.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a recovery code was not redeemed.</returns>
        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            var str = _localizer["RecoveryCodeRedemptionFailed"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.RecoveryCodeRedemptionFailed();
            }

            return new IdentityError
            {
                Code = nameof(RecoveryCodeRedemptionFailed),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating an external login is already associated with an account.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating an external login is already associated with an account.</returns>
        public override IdentityError LoginAlreadyAssociated()
        {
            var str = _localizer["LoginAlreadyAssociated"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.LoginAlreadyAssociated();
            }

            return new IdentityError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified user <paramref name="userName"/> is invalid.
        /// </summary>
        /// <param name="userName">The user name that is invalid.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified user <paramref name="userName"/> is invalid.</returns>
        public override IdentityError InvalidUserName(string? userName)
        {
            var str = _localizer["InvalidUserName", userName ?? ""];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.InvalidUserName(userName);
            }

            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is invalid.
        /// </summary>
        /// <param name="email">The email that is invalid.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is invalid.</returns>
        public override IdentityError InvalidEmail(string? email)
        {
            var str = _localizer["InvalidEmail", email ?? ""];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.InvalidEmail(email);
            }

            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="userName"/> already exists.
        /// </summary>
        /// <param name="userName">The user name that already exists.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="userName"/> already exists.</returns>
        public override IdentityError DuplicateUserName(string userName)
        {
            var str = _localizer["DuplicateUserName", userName];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.DuplicateUserName(userName);
            }

            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is already associated with an account.
        /// </summary>
        /// <param name="email">The email that is already associated with an account.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is already associated with an account.</returns>
        public override IdentityError DuplicateEmail(string email)
        {
            var str = _localizer["DuplicateEmail", email];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.DuplicateEmail(email);
            }

            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="role"/> name is invalid.
        /// </summary>
        /// <param name="role">The invalid role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specific role <paramref name="role"/> name is invalid.</returns>
        public override IdentityError InvalidRoleName(string? role)
        {
            var str = _localizer["InvalidRoleName", role ?? ""];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.InvalidRoleName(role);
            }

            return new IdentityError
            {
                Code = nameof(InvalidRoleName),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="role"/> name already exists.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specific role <paramref name="role"/> name already exists.</returns>
        public override IdentityError DuplicateRoleName(string role)
        {
            var str = _localizer["DuplicateRoleName", role];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.DuplicateRoleName(role);
            }

            return new IdentityError
            {
                Code = nameof(DuplicateRoleName),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a user already has a password.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a user already has a password.</returns>
        public override IdentityError UserAlreadyHasPassword()
        {
            var str = _localizer["UserAlreadyHasPassword"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.UserAlreadyHasPassword();
            }

            return new IdentityError
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating user lockout is not enabled.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating user lockout is not enabled.</returns>
        public override IdentityError UserLockoutNotEnabled()
        {
            var str = _localizer["UserLockoutNotEnabled"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.UserLockoutNotEnabled();
            }

            return new IdentityError
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a user is already in the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a user is already in the specified <paramref name="role"/>.</returns>
        public override IdentityError UserAlreadyInRole(string role)
        {
            var str = _localizer["UserAlreadyInRole", role];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.UserAlreadyInRole(role);
            }

            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a user is not in the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a user is not in the specified <paramref name="role"/>.</returns>
        public override IdentityError UserNotInRole(string role)
        {
            var str = _localizer["UserNotInRole", role];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.UserNotInRole(role);
            }

            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password of the specified <paramref name="length"/> does not meet the minimum length requirements.
        /// </summary>
        /// <param name="length">The length that is not long enough.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a password of the specified <paramref name="length"/> does not meet the minimum length requirements.</returns>
        public override IdentityError PasswordTooShort(int length)
        {
            var str = _localizer["PasswordTooShort", length];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordTooShort(length);
            }

            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password does not meet the minimum number <paramref name="uniqueChars"/> of unique chars.
        /// </summary>
        /// <param name="uniqueChars">The number of different chars that must be used.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a password does not meet the minimum number <paramref name="uniqueChars"/> of unique chars.</returns>
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            var str = _localizer["PasswordRequiresUniqueChars", uniqueChars];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordRequiresUniqueChars(uniqueChars);
            }

            return new IdentityError
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a non-alphanumeric character, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a non-alphanumeric character.</returns>
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            var str = _localizer["PasswordRequiresNonAlphanumeric"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordRequiresNonAlphanumeric();
            }

            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a numeric character, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a numeric character.</returns>
        public override IdentityError PasswordRequiresDigit()
        {
            var str = _localizer["PasswordRequiresDigit"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordRequiresDigit();
            }

            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a lower case letter, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a lower case letter.</returns>
        public override IdentityError PasswordRequiresLower()
        {
            var str = _localizer["PasswordRequiresLower"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordRequiresLower();
            }

            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = str.Value
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain an upper case letter, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain an upper case letter.</returns>
        public override IdentityError PasswordRequiresUpper()
        {
            var str = _localizer["PasswordRequiresUpper"];
            if (str.SearchedLocation == null || str.ResourceNotFound)
            {
                return base.PasswordRequiresUpper();
            }

            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = str.Value
            };
        }
    }
}
