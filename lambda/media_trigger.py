import boto3
import json
import os

def lambda_handler(event, context):
    # CẤU HÌNH
    MEDIACONVERT_ENDPOINT = 'your-mediaconvert-endpoint'
    ROLE_ARN = 'your-role-arn' 
    
    # 1. Lấy thông tin file upload
    bucket_name = event['Records'][0]['s3']['bucket']['name'] 
    source_key = event['Records'][0]['s3']['object']['key']   
    file_id = source_key.split('/')[-1].split('.')[0]
    
    source_s3 = f"s3://{bucket_name}/{source_key}"
    # File Master sẽ là: .../{file_id}/{file_id}.m3u8
    destination_s3 = f"s3://{bucket_name}/courses/hls/{file_id}/{file_id}"
    
    client = boto3.client('mediaconvert', endpoint_url=MEDIACONVERT_ENDPOINT)
    
    # 2. CẤU HÌNH 3 ĐỘ PHÂN GIẢI (480p, 720p, 1080p)
    job_settings = {
        "Inputs": [{"FileInput": source_s3, "AudioSelectors": {"Audio Selector 1": {"DefaultSelection": "DEFAULT"}}}],
        "OutputGroups": [{
            "Name": "HLS_Group",
            "OutputGroupSettings": {
                "Type": "HLS_GROUP_SETTINGS",
                "HlsGroupSettings": {
                    "SegmentLength": 6,   
                    "MinSegmentLength": 0,
                    "Destination": destination_s3
                }
            },
            "Outputs": [
                # 1080p 
                {
                    "NameModifier": "_1080p",
                    "VideoDescription": {
                        "Width": 1920, "Height": 1080,
                        "CodecSettings": {
                            "Codec": "H_264",
                            "H264Settings": {
                                "RateControlMode": "QVBR",
                                "MaxBitrate": 4500000,
                                "QvbrSettings": { "QvbrQualityLevel": 8 }
                            }
                        }
                    },
                    "AudioDescriptions": [{
                        "CodecSettings": {
                            "Codec": "AAC",
                            "AacSettings": {
                                "Bitrate": 128000,
                                "CodingMode": "CODING_MODE_2_0",
                                "SampleRate": 48000
                            }
                        }
                    }],
                    "ContainerSettings": {"Container": "M3U8"}
                },
                # 720p
                {
                    "NameModifier": "_720p",
                    "VideoDescription": {
                        "Width": 1280, "Height": 720,
                        "CodecSettings": {
                            "Codec": "H_264",
                            "H264Settings": {
                                "RateControlMode": "QVBR",
                                "MaxBitrate": 2500000,
                                "QvbrSettings": { "QvbrQualityLevel": 7 }
                            }
                        }
                    },
                    "AudioDescriptions": [{
                        "CodecSettings": {
                            "Codec": "AAC",
                            "AacSettings": {
                                "Bitrate": 96000,
                                "CodingMode": "CODING_MODE_2_0",
                                "SampleRate": 48000
                            }
                        }
                    }],
                    "ContainerSettings": {"Container": "M3U8"}
                },
                # 480p
                {
                    "NameModifier": "_480p",
                    "VideoDescription": {
                        "Width": 854, "Height": 480,
                        "CodecSettings": {
                            "Codec": "H_264",
                            "H264Settings": {
                                "RateControlMode": "QVBR",
                                "MaxBitrate": 1000000,
                                "QvbrSettings": { "QvbrQualityLevel": 7 }
                            }
                        }
                    },
                    "AudioDescriptions": [{
                        "CodecSettings": {
                            "Codec": "AAC",
                            "AacSettings": {
                                "Bitrate": 64000, # Tiết kiệm tối đa
                                "CodingMode": "CODING_MODE_2_0",
                                "SampleRate": 48000
                            }
                        }
                    }],
                    "ContainerSettings": {"Container": "M3U8"}
                }
            ]
        }]
    }

    try:
        response = client.create_job(
            Role=ROLE_ARN,
            Settings=job_settings,
            UserMetadata={ 
                "file_id": file_id,
                "original_key": source_key 
            } 
        )
        print(f"Job Created: {response['Job']['Id']}")
        return {'statusCode': 200, 'body': json.dumps("Job Created")}
    except Exception as e:
        print(f"Error: {e}")
        raise e