
# Magic Lambda Mail

 SMTP and POP3 helpers for Magic.

 Contains POP3 and SMTP slots helpers for Magi.

1. **[mail.pop3.fetch]** - Fetches emails form some specific POP3 server
2. **[mail.smtp.send]** - Sends email over SMTP server

Both of the above slots have async (wait.) overrides for executing asynchronously.

## Sending email(s)

```
mail.smtp.send
   server
      host:foo.com
      port:123
      secure:true
      username:xxx
      password:yyy
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content
```

You can send multiple **[message]** objects at the same time, using the same SMTP connection and credentials.
The entirety of the **[server]** node above is optional, and if not given, will be fetched from your
configuration settings, using the `IConfiguration` object, which is given through dependency injection.
You can also override only one or two parts in your **[server]** segment above, and have the system
read the rest of the settings from your application's configuration. Below are the keys used to fetch
configuration settings for SMTP connections, if not explicitly given as part of invocation.

* magic.smtp.host
* magic.smtp.port
* magic.smtp.secure
* magic.smtp.username
* magic.smtp.password

An example of how your configuration might look like, if you choose to use configuration settings,
instead of having to supply server configuration every time you invoke the slot.

```json
{
   "magic":{
      "smtp":{
        "host":"smtp.gmail.com",
        "port":465,
        "secure":true,
        "username":"username@gmail.com",
        "password":"gmail-password",
      }
   }
}
```

If you exchange the above username/password combination, and open your GMail account for _"insecure apps"_,
the above will actually allow you to send emails using your GMail account.

To construct your email's **[message]** part, [see Magic Mime for details](https://github.com/polterguy/magic.lambda.mime).

## Retrieving emails

```
mail.pop3.fetch
   server
      host:foo.com
      port:123
      secure:true
      username:xxx
      password:yyy
   max:int:50
   .lambda
      /*
       * Some lambda object invoked once for every email fetched.
       * Given message as [.message] node structured as lambda.
       */
```

Just like its SMTP counterpart, parts of, or the entirety of the above **[server]** node is optional.
Below are the keys used to fetch configuration settings for SMTP connections, if not explicitly given
as part of invocation.

* magic.pop3.host
* magic.pop3.port
* magic.pop3.secure
* magic.pop3.username
* magic.pop3.password

Your **[.lambda]** callback will be invoked with a single **[.message]** node, containing the
structured version of the MIME message wrapping the actual email message. Refer to
[see Magic Mime for details](https://github.com/polterguy/magic.lambda.mime) to understand this
structure.
