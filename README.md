# Simplest CRON Expression Library
.NET data type for CRON expressions.

This is an open-source code example for CRON expressions in .NET. I made this project because:

1. I was bord and looking for a challenge
2. I don't like what I see in the current CRON open-source projects like HangfireIO/Cronos which uses unsafe string manipulation and BIT-wise logic; which is intentionally confusing and hard to follow in my opinion
3. I needed a bar bone CRON expression class that is divorced from any scheduling implementation; which is trivial to self-implement
4. I needed a CRON library that has a small code footprint

Contributors are welcomed.

# Code Example
CronExpression is simple class construct that allows you to project the next interval of any given point in time.  In addition, it provides the ability to determin the time span until the next interval.

## Projecting Next Interval

## Determining Timeout

