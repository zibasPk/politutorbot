﻿using System.Text.RegularExpressions;

namespace Bot.Constants;

public class RegularExpr
{
  public const string StudentCode = "^[1-9][0-9]{5}$";
  public const string PersonCode = "^[1-9][0-9]{5}$";
  public const string TelegramUserId = "^[1-9][0-9]{1,20}$";
}