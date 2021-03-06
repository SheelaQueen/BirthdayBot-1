﻿Imports System.Text
Imports Discord
Imports Discord.WebSocket

Friend Class HelpInfoCommands
    Inherits CommandsCommon

    Private ReadOnly _helpEmbed As Embed
    Private ReadOnly _helpConfigEmbed As Embed

    Sub New(inst As BirthdayBot, db As Configuration, client As DiscordShardedClient)
        MyBase.New(inst, db)
        Dim embeds = BuildHelpEmbeds()
        _helpEmbed = embeds.Item1
        _helpConfigEmbed = embeds.Item2
    End Sub

    Public Overrides ReadOnly Property Commands As IEnumerable(Of (String, CommandHandler))
        Get
            Return New List(Of (String, CommandHandler)) From {
                ("help", AddressOf CmdHelp),
                ("help-config", AddressOf CmdHelpConfig),
                ("help-tzdata", AddressOf CmdHelpTzdata),
                ("help-message", AddressOf CmdHelpMessage),
                ("info", AddressOf CmdInfo)
            }
        End Get
    End Property

    Private Function BuildHelpEmbeds() As (Embed, Embed)
        Dim cpfx = $"●`{CommandPrefix}"
        ' Normal section
        Dim cmdField As New EmbedFieldBuilder With {
            .Name = "Commands",
            .Value =
                $"{cpfx}help`, `{CommandPrefix}info`, `{CommandPrefix}help-tzdata`" + vbLf +
                $" » Help and informational messages." + vbLf +
                $"{cpfx}recent` and `{CommandPrefix}upcoming`" + vbLf +
                $" » Lists recent and upcoming birthdays." + vbLf +
                $"{cpfx}set (date) [zone]`" + vbLf +
                $" » Registers your birth date. Time zone is optional." + vbLf +
                $" »» Examples: `{CommandPrefix}set jan-31`, `{CommandPrefix}set 15-aug America/Los_Angeles`." + vbLf +
                $"{cpfx}zone (zone)`" + vbLf +
                $" » Sets your local time zone. See `{CommandPrefix}help-tzdata`." + vbLf +
                $"{cpfx}remove`" + vbLf +
                $" » Removes your birthday information from this bot." + vbLf +
                $"{cpfx}when (user)`" + vbLf +
                $" » Displays birthday information of the given user."
        }
        Dim cmdModField As New EmbedFieldBuilder With {
            .Name = "Moderator-only commands",
            .Value =
                $"{cpfx}config`" + vbLf +
                $" » Edit bot configuration. See `{CommandPrefix}help-config`." + vbLf +
                $"{cpfx}list`" + vbLf +
                $" » Exports all birthdays to file. Accepts `csv` as a parameter." + vbLf +
                $"{cpfx}override (user ping or ID) (command w/ parameters)`" + vbLf +
                " » Perform certain commands on behalf of another user."
        }
        Dim helpRegular As New EmbedBuilder
        helpRegular.AddField(cmdField)
        helpRegular.AddField(cmdModField)

        ' Manager section
        Dim mpfx = cpfx + "config "
        Dim configField1 As New EmbedFieldBuilder With {
            .Name = "Basic settings",
            .Value =
                $"{mpfx}role (role name or ID)`" + vbLf +
                " » Sets the role to apply to users having birthdays." + vbLf +
                $"{mpfx}channel (channel name or ID)`" + vbLf +
                " » Sets the announcement channel. Leave blank to disable." + vbLf +
                $"{mpfx}message (message)`, `{CommandPrefix}config messagepl (message)`" + vbLf +
                $" » Sets a custom announcement message. See `{CommandPrefix}help-message`." + vbLf +
                $"{mpfx}ping (off|on)`" + vbLf +
                $" » Sets whether to ping the respective users in the announcement message." + vbLf +
                $"{mpfx}zone (time zone name)`" + vbLf +
                $" » Sets the default server time zone. See `{CommandPrefix}help-tzdata`."
        }
        Dim configField2 As New EmbedFieldBuilder With {
            .Name = "Access management",
            .Value =
                $"{mpfx}modrole (role name, role ping, or ID)`" + vbLf +
                " » Establishes a role for bot moderators. Grants access to `bb.config` and `bb.override`." + vbLf +
                $"{mpfx}block/unblock (user ping or ID)`" + vbLf +
                " » Prevents or allows usage of bot commands to the given user." + vbLf +
                $"{mpfx}moderated on/off`" + vbLf +
                " » Prevents or allows using commands for all members excluding moderators."
        }

        Dim helpConfig As New EmbedBuilder
        helpConfig.Author = New EmbedAuthorBuilder() With {.Name = $"{CommandPrefix}config subcommands"}
        helpConfig.Description = "All the following subcommands are only usable by moderators and server managers."
        helpConfig.AddField(configField1)
        helpConfig.AddField(configField2)

        Return (helpRegular.Build(), helpConfig.Build())
    End Function

    Private Async Function CmdHelp(param As String(), reqChannel As SocketTextChannel, reqUser As SocketGuildUser) As Task
        Await reqChannel.SendMessageAsync(embed:=_helpEmbed)
    End Function

    Private Async Function CmdHelpConfig(param As String(), reqChannel As SocketTextChannel, reqUser As SocketGuildUser) As Task
        Await reqChannel.SendMessageAsync(embed:=_helpConfigEmbed)
    End Function

    Private Async Function CmdHelpTzdata(param As String(), reqChannel As SocketTextChannel, reqUser As SocketGuildUser) As Task
        Const tzhelp = "You may specify a time zone in order to have your birthday recognized with respect to your local time. " +
            "This bot only accepts zone names from the IANA Time Zone Database (a.k.a. Olson Database)." + vbLf + vbLf +
            "These names can be found at the following link, under the 'TZ database name' column: " +
            "https://en.wikipedia.org/wiki/List_of_tz_database_time_zones"
        Dim embed As New EmbedBuilder
        embed.AddField(New EmbedFieldBuilder() With {
            .Name = "Time Zone Support",
            .Value = tzhelp
        })
        Await reqChannel.SendMessageAsync(embed:=embed.Build())
    End Function

    Private Async Function CmdHelpMessage(param As String(), reqChannel As SocketTextChannel, reqUser As SocketGuildUser) As Task
        Const msghelp = "The `message` and `messagepl` subcommands allow for editing the message sent into the announcement " +
            "channel (defined with `{0}config channel`). This feature is separated across two commands:" + vbLf +
            "●`{0}config message`" + vbLf + "●`{0}config messagepl`" + vbLf +
            "The first command sets the message to be displayed when *one* user is having a birthday. The second command sets the " +
            "message for when *two or more* users are having birthdays ('pl' means plural). If only one of the two custom messages " +
            "are defined, it will be used for both cases." + vbLf + vbLf +
            "To further allow customization, you may place the token `%n` in your message to specify where the name(s) should appear." +
            vbLf + "Leave the parameter blank to clear or reset the message to its default value."
        Const msghelp2 = "As examples, these are the default announcement messages used by this bot:" + vbLf +
            "`message`: {0}" + vbLf + "`messagepl`: {1}"
        Dim embed As New EmbedBuilder
        embed.AddField(New EmbedFieldBuilder() With {
            .Name = "Custom announcement message",
            .Value = String.Format(msghelp, CommandPrefix)
        })
        embed.AddField(New EmbedFieldBuilder() With {
            .Name = "Examples",
            .Value = String.Format(msghelp2, BirthdayRoleUpdate.DefaultAnnounce, BirthdayRoleUpdate.DefaultAnnouncePl)
        })
        Await reqChannel.SendMessageAsync(embed:=embed.Build())
    End Function

    Private Async Function CmdInfo(param As String(), reqChannel As SocketTextChannel, reqUser As SocketGuildUser) As Task
        ' Bot stats field
        Dim strStats As New StringBuilder
        Dim asmnm = Reflection.Assembly.GetExecutingAssembly.GetName()
        strStats.AppendLine("Birthday Bot v" + asmnm.Version.ToString(3))
        strStats.AppendLine("Server count: " + Discord.Guilds.Count.ToString())
        strStats.AppendLine("Shard #" + Discord.GetShardIdFor(reqChannel.Guild).ToString())
        strStats.AppendLine("Uptime: " + BotUptime())

        ' TODO fun stats
        ' current birthdays, total names registered, unique time zones

        Dim embed As New EmbedBuilder With {
            .Author = New EmbedAuthorBuilder() With {
                .Name = "Thank you for using Birthday Bot!",
                .IconUrl = Discord.CurrentUser.GetAvatarUrl()
            },
            .Description = "For more information regarding support, data retention, privacy, and other details, please refer to: " +
                "https://github.com/NoiTheCat/BirthdayBot/blob/master/Readme.md" + vbLf + vbLf +
                "This bot is provided for free, without any intention to add premium, pay-only features. " +
                "If you find this bot helpful, please consider contributing towards my operating costs " +
                "via Patreon: https://www.patreon.com/noibots."
        }
        Dim verstr = Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
        embed.AddField(New EmbedFieldBuilder With {
            .Name = "Statistics",
            .Value = strStats.ToString()
        })
        Await reqChannel.SendMessageAsync(embed:=embed.Build())
    End Function
End Class
