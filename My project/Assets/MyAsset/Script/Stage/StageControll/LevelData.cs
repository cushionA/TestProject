using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData",menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{

    /// <summary>
    /// ���x���}�l�[�W���[���烍�[�h����Ƃ��Ɏg��
    /// </summary>
    public string mapName;

    /// <summary>
    /// ���̃}�b�v�f�[�^���ύX����Ă邩�B�^�Ȃ炳��Ă�
    /// �ύX����ĂȂ��ꍇ�Z�[�u�������[�h�����Ȃ�
    /// ���x���}�l�[�W���[�ɖ��ߍ��܂ꂽ�����l�̂܂�
    /// �����Ă��i���ω��̔��Ƃ��̓t���O�ŊǗ�����̂ł�
    /// ���ႠisChange���^�̐����������X�g���ă��׃}�l�Ɏ������邩�H
    /// �����ق�ƂɕK�v���H
    /// ������}�b�v�����ŗǂ��˗��΂�
    /// </summary>
    public bool isChanged;






    /// <summary>
    /// �^�U�l��ۑ����āA�^�̕��𖳌������Ă���
    /// �������A�ŏ��ɃI�u�W�F�N�g�����������̔ԍ��Ŗ₢���킹�Ė����ł���ꍇ�̃A�N�V���������Ă���
    /// �����J�����h�A�Ȃǉi���I�ɕς��Ȃ����̂ɂ��Ă͕ʂɊǗ����悤�B�Ƃ���������̓t���O�ł���
    /// �t���O�����Ă邩�m�F���Ă���
    /// �N�����͌��݂���}�b�v�̂��ꂾ�������ɂ񂷂�
    /// �j�󂳂ꂽ���̓I�u�W�F�N�g���炿���Ɛ\������
    /// </summary>
    [Header("���݂̃}�b�v�̃I�u�W�F�N�g�̏�")]
    [ES3Serializable]
    public bool[] disenableObjects;

    /// <summary>
    /// �^�U�l��ۑ����āA�^�̕��𖳌������Ă���
    /// �������A�ŏ��ɃG�l�~�[�������̔ԍ��Ŗ₢���킹�Ė����ł���ꍇ�̃A�N�V���������Ă���
    /// �{�X�⃌�A�G�l�~�[�Ȃǉi���I�ɕς��Ȃ����̂ɂ��Ă͕ʂɊǗ����悤�B�Ƃ���������̓t���O�ł���
    /// �t���O�����Ă邩�m�F���Ă���
    /// �N�����͌��݂���}�b�v�̂��ꂾ�������ɂ񂷂�
    /// ���񂾓G�Ɋւ��Ă͂����Ǝ��ʂƂ��Ɏ����̔ԍ��Ő\������
    /// </summary>
    [Header("���݂̃}�b�v�̓G�̏�")]
    [ES3Serializable]
    public bool[] disenableEnemy;


}
