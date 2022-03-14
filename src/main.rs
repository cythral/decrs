use std::env;
use std::str;
use bytes::Bytes;
use rusoto_core::Region;
use rusoto_kms::{Kms,KmsClient,DecryptRequest};

macro_rules! error {
    ($fmt:expr) => ({ eprintln!(concat!($fmt, "\n")); std::process::exit(1) });
    ($fmt:expr, $arg:tt) => ({ eprintln!(concat!($fmt, "\n"), $($arg)*); std::process::exit(1) });
}

fn get_ciphertext() -> Bytes
{
    let args: Vec<String> = env::args().collect();
    let ciphertext = match args.get(1)
    {
        Some(arg) => arg.trim().clone(),
        None => error!("Expected 1 argument, got 0."),
    };

    let ciphertext_blob = match base64::decode(ciphertext)
    {
        Ok(blob) => blob,
        Err(_) => error!("Expected argument 1 to be a valid base64 encoded value."),
    };

    return Bytes::from(ciphertext_blob);
}

#[tokio::main]
async fn main()
{
    let ciphertext_blob = get_ciphertext();
    let client = KmsClient::new(Region::UsEast1);
    let decrypt_request = DecryptRequest {
        ciphertext_blob: ciphertext_blob,
        encryption_algorithm: None,
        encryption_context: None,
        grant_tokens: None,
        key_id: None,
    };

    let decrypt_response_result = client.decrypt(decrypt_request).await;
    if decrypt_response_result.is_err()
    {
        error!("Failed to decrypt the ciphertext.  Possibly bad credentials.");
    }

    let decrypt_response = decrypt_response_result.unwrap();
    let decrypted_bytes = decrypt_response.plaintext.unwrap();
    let decrypted_string = str::from_utf8(decrypted_bytes.as_ref()).unwrap();
    print!("{}", decrypted_string);
}
