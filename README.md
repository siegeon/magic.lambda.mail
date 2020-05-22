
# Magic Lambda Mail

 SMTP and POP3 helpers for Magic.

 Contains one slot called **[wait.mail.pop3.fetch]** which basically fetches emails form some specific POP3 server. Example of usage can be found below.

 ```
 wait.mail.pop3.fetch
    server:some.host.com
    port:int:55
    secure:bool:true
    username:foo-bar-username
    password:foo-bar-password
    max:int:50
    .lambda
       /*
        * Some lambda object invoked once for every email fetched.
        */
 ```

 The following arguments are optional.

 * secure (defaults to false)
 * username (no authentication if not given)
 * password (no authentication if not given)
 * max (defaults to 50 if not given)

 If **[secure]** is true, connection to POP3 server will be established using TLS/SSL connection - Otherwise plaintext connection will be used. Your **[.lambda]** callback
 will be invoked with a single **[.message]** node, containing the structured version of the MIME message wrapping the actual email. Refer to `magic.lambda.mime` to understand
 this structure.
