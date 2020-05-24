
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
read the rest of the settings from your application's configuration. In addition, the **[from]** node is also
optional, assuming you have a default `from` configured in your configuration settings. Below are the keys used
to fetch configuration settings for SMTP connections, and from object, if not explicitly given as part of
invocation.

* magic.smtp.host
* magic.smtp.port
* magic.smtp.secure
* magic.smtp.username
* magic.smtp.password
* magic.smtp.from.name
* magic.smtp.from.address

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
        "from": {
           "name":"John Doe",
           "address":"john@doe.com",
        }
      }
   }
}
```

**FYI** - If you exchange the above username/password combination, and open your GMail account for _"insecure apps"_,
the above will actually allow you to send emails using your GMail account.

Assuming you have the above somewhere in your configuration, you can construct and send an email using something
like the following. Which probably makes things more convenient, allowing you to avoid bothering about connection
settings, from addresses, etc - And leave this as a part of your Azure transformation pipeline(s), or something
similar.

```
mail.smtp.send
   message
      to
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content
```

You can also add **[cc]** and **[bcc]** recipients for your emails, using the same structure you're using for **[to]**.

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
   raw:bool:false
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

Notice, if **[raw]** is true, the message will _not_ be parsed and turned into a structural lambda object,
but passed into your **[.lambda]** as its raw MIME message instead. The default value for **[raw]** is false.

Your **[.lambda]** callback will be invoked with a single **[.message]** node, containing the
structured version of the MIME message wrapping the actual email message. Refer to
[see Magic Mime for details](https://github.com/polterguy/magic.lambda.mime) to understand this
structure.
