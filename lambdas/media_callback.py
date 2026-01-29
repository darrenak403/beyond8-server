import json
import urllib3
from urllib3.util import Retry

def lambda_handler(event, context):
    # Domain CloudFront
    CLOUDFRONT_DOMAIN = "d30z0qh7rhzgt8.cloudfront.net" 
    # API Callback 
    BACKEND_URL = "backend-url/api/v1/lessons/video/callback"
    
    # METADATA
    detail = event.get('detail', {})
    user_metadata = detail.get('userMetadata', {})
    original_key = user_metadata.get('original_key')
    file_id = user_metadata.get('file_id')
    
    output_groups = detail.get('outputGroupDetails', [])
    
    # KHỞI TẠO OBJECT KẾT QUẢ
    transcoding_data = {
        "variants": [] 
    }
    
    if output_groups:
        outputs = output_groups[0].get('outputDetails', [])
        
        # XỬ LÝ & TẠO LINK MASTER
        if outputs and file_id:
            first_path = outputs[0]['outputFilePaths'][0]             
            full_path_clean = first_path.replace("s3://", "")
            parts = full_path_clean.split("/", 1)
            
            if len(parts) > 1:
                bucket_name = parts[0]
                variant_key = parts[1]
                folder_path = variant_key.rsplit('/', 1)[0]
                master_key = f"{folder_path}/{file_id}.m3u8"
                
                # Tạo URL Master
                master_url = f"https://{CLOUDFRONT_DOMAIN}/{master_key}"
                
                # THÊM MASTER VÀO LIST VARIANTS
                transcoding_data["variants"].append({
                    "quality": "master",
                    "url": master_url
                })

        # XỬ LÝ CÁC FILE CON
        for out in outputs:
            s3_url = out['outputFilePaths'][0]
            obj_key = s3_url.replace("s3://", "").split("/", 1)[1]

            final_url = f"https://{CLOUDFRONT_DOMAIN}/{obj_key}"
            height = str(out.get('videoDetails', {}).get('heightInPixels', 'Auto'))
            
            transcoding_data["variants"].append({
                "quality": height, # "1080", "720", "480"
                "url": final_url
            })

    # GỬI VỀ BACKEND
    retry_strategy = Retry(
        total=5,
        backoff_factor=1,
        status_forcelist=[429, 500, 502, 503, 504],
        allowed_methods=["POST"]
    )

    http = urllib3.PoolManager(retries=retry_strategy)  

    payload = {
        "originalKey": original_key,
        "transcodingData": transcoding_data
    }
    
    try:
        r = http.request('POST', BACKEND_URL, 
                         body=json.dumps(payload), 
                         headers={'Content-Type': 'application/json'})
        
        print(f"Callback Status: {r.status}")
        return {"status": r.status}
    except Exception as e:
        print(f"Callback Failed after retries: {e}")
        raise e